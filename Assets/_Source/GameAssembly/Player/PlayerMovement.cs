using GameAssembly.Player.Data;
using GameAssembly.Player.Input;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace GameAssembly.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;

        [SerializeField] private LayerMask groundLayers = ~0;
        [SerializeField, Min(0.01f)] private float groundCheckDistance = 0.2f;
        [SerializeField, Range(0f, 89f)] private float maxSlopeAngle = 55f;
        [SerializeField, Min(0f)] private float groundStickSpeed = 2f;

        [SerializeField, Min(0f)] private float maxStepHeight = 0.6f;
        [SerializeField, Min(0.01f)] private float stepCheckDistance = 0.1f;
        [SerializeField, Min(0.001f)] private float stepClearance = 0.02f;

        private readonly RaycastHit[] _groundHits = new RaycastHit[16];
        private readonly RaycastHit[] _stepHits = new RaycastHit[16];
        private readonly List<Vector3> _wallNormals = new();
        private Collider _playerCollider;
        private bool _hasStepSupport;

        [Inject] private PlayerCamera _playerCamera;
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private PlayerState _playerState;
        [Inject] private PlayerInput _playerInput;
        [Inject] private PlayerStamina _playerStamina;

        private void Awake()
        {
            _playerCollider = rb.GetComponent<Collider>();
        }

        public void Teleport(Vector3 position)
        {
            _hasStepSupport = false;
            _wallNormals.Clear();
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.position = position; 
            transform.position = position;
        }

        private void FixedUpdate()
        {
            if (_playerState.IsInTwoD())
            {
                _hasStepSupport = false;
                _playerStamina.Tick(false, Time.fixedDeltaTime);
                _wallNormals.Clear();
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                return;
            }

            var canMove = _playerState.GetCurrentState() == PlayerInteractStates.FREE ||
                          _playerState.GetCurrentState() == PlayerInteractStates.MOVEMENT_ONLY;
            var inputVector = canMove ? _playerInput.ReadMovementVector() : Vector2.zero;
            var isSprinting = inputVector.sqrMagnitude > 0.001f && _playerInput.IsSprintPressed() &&
                              _playerStamina.CanSprint;
            var currentSpeed = isSprinting ? _playerSettings.SprintSpeed : _playerSettings.Speed;

            if (_playerState.IsInShelter())
                currentSpeed *= _playerSettings.ShelterSpeedMultiplier;

            var moveVector = inputVector * currentSpeed;

            var cameraYRotation = Quaternion.Euler(0f, _playerCamera.transform.eulerAngles.y, 0f);
            var worldMove = cameraYRotation * new Vector3(moveVector.x, 0f, moveVector.y);
            var isGrounded = TryGetGroundNormal(out var groundNormal);
            var steppedUp = isGrounded && TryStepUp(worldMove);

            if (steppedUp)
            {
                _wallNormals.Clear();
                groundNormal = Vector3.up;
            }
            else
                worldMove = RemoveMovementIntoWalls(worldMove);

            _hasStepSupport = steppedUp || _hasStepSupport && isGrounded && IsSupportedOnStepEdge();
            _playerStamina.Tick(isSprinting && worldMove.sqrMagnitude > 0.001f, Time.fixedDeltaTime);
            if (_hasStepSupport)
            {
                // Cancel gravity for this simulation step instead of pushing the capsule off the stair nose.
                var verticalVelocity = rb.useGravity ? -Physics.gravity.y * Time.fixedDeltaTime : 0f;
                rb.linearVelocity = new Vector3(worldMove.x, verticalVelocity, worldMove.z);
            }
            else if (isGrounded)
            {
                var groundMove = Vector3.ProjectOnPlane(worldMove, groundNormal).normalized * worldMove.magnitude;
                rb.linearVelocity = groundMove - groundNormal * groundStickSpeed;
            }
            else
                rb.linearVelocity = new Vector3(worldMove.x, Mathf.Min(rb.linearVelocity.y, 0f), worldMove.z);
        }

        private void OnCollisionEnter(Collision collision) => RegisterWallContacts(collision);
        private void OnCollisionStay(Collision collision) => RegisterWallContacts(collision);

        private void OnDisable()
        {
            _wallNormals.Clear();
            _hasStepSupport = false;
        }

        private bool IsSupportedOnStepEdge()
        {
            var bounds = _playerCollider.bounds;
            var feet = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            if (TryStepRaycast(feet + Vector3.up * stepClearance, Vector3.down,
                    stepClearance * 2f, out var floor) && floor.normal.y >= Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad))
                return false;

            var radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
            var bottom = feet + Vector3.up * (radius + stepClearance);
            var top = new Vector3(bounds.center.x, bounds.max.y - radius + stepClearance, bounds.center.z);
            var hitCount = Physics.CapsuleCastNonAlloc(bottom, top, radius + stepClearance, Vector3.down,
                _stepHits, stepClearance * 3f, groundLayers, QueryTriggerInteraction.Ignore);

            for (var i = 0; i < hitCount; i++)
            {
                if (_stepHits[i].rigidbody != rb && _stepHits[i].normal.y > 0.001f)
                    return true;
            }

            return false;
        }

        private bool TryStepUp(Vector3 movement)
        {
            if (maxStepHeight <= 0f || movement.sqrMagnitude < 0.001f || !_playerCollider ||
                !_playerCollider.enabled || rb.linearVelocity.y < -groundStickSpeed - 0.5f)
                return false;

            var bounds = _playerCollider.bounds;
            var direction = movement.normalized;
            var distance = movement.magnitude * Time.fixedDeltaTime;
            var radius = Mathf.Min(bounds.extents.x, bounds.extents.z);
            var feet = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

            if (!TryStepRaycast(feet + Vector3.up * stepClearance, direction,
                    radius + stepCheckDistance + distance, out var obstacle) || Mathf.Abs(obstacle.normal.y) > 0.1f)
                return false;

            var topOrigin = feet + direction * (obstacle.distance + stepCheckDistance) +
                            Vector3.up * (maxStepHeight + stepClearance);

            if (!TryStepRaycast(topOrigin, Vector3.down, maxStepHeight + stepClearance * 2f, out var surface) ||
                surface.normal.y < Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad))
                return false;

            var height = surface.point.y - feet.y;
            if (height <= stepClearance || height > maxStepHeight)
                return false;

            var bottom = feet + Vector3.up * radius;
            var top = new Vector3(bounds.center.x, bounds.max.y - radius, bounds.center.z);
            var lift = maxStepHeight + stepClearance;
            var offset = direction * distance + Vector3.up * lift;
            // Check the whole capsule: on narrow stairs its front can already overlap the next tread.
            var hitCount = Physics.CapsuleCastNonAlloc(bottom + offset, top + offset, radius + stepClearance,
                Vector3.down, _stepHits, lift + stepClearance, groundLayers, QueryTriggerInteraction.Ignore);
            var closestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _stepHits[i];
                if (hit.rigidbody == rb || hit.normal.y <= 0.001f || hit.distance >= closestDistance)
                    continue;

                closestDistance = hit.distance;
            }

            var rise = lift - closestDistance;
            if (rise <= 0f || rise > maxStepHeight + stepClearance)
                return false;

            var clearance = Vector3.up * stepClearance;
            if (HasStepCapsuleObstacle(bottom + clearance, top + clearance, radius, Vector3.up,
                    Mathf.Max(0f, rise - stepClearance)) ||
                HasStepCapsuleObstacle(bottom + Vector3.up * rise, top + Vector3.up * rise,
                    radius, direction, distance))
                return false;

            rb.position += Vector3.up * rise;
            return true;
        }

        private bool TryStepRaycast(Vector3 origin, Vector3 direction, float distance, out RaycastHit closestHit)
        {
            closestHit = default;
            var hitCount = Physics.RaycastNonAlloc(origin, direction, _stepHits, distance,
                groundLayers, QueryTriggerInteraction.Ignore);
            var closestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _stepHits[i];
                if (hit.rigidbody == rb || hit.distance >= closestDistance)
                    continue;

                closestDistance = hit.distance;
                closestHit = hit;
            }

            return closestDistance < float.MaxValue;
        }

        private bool HasStepCapsuleObstacle(Vector3 bottom, Vector3 top, float radius, Vector3 direction,
            float distance)
        {
            var hitCount = Physics.CapsuleCastNonAlloc(bottom, top, radius, direction, _stepHits, distance,
                groundLayers, QueryTriggerInteraction.Ignore);

            for (var i = 0; i < hitCount; i++)
            {
                if (_stepHits[i].rigidbody != rb && Vector3.Dot(_stepHits[i].normal, direction) < -0.001f)
                    return true;
            }

            return false;
        }

        private void RegisterWallContacts(Collision collision)
        {
            var minGroundDot = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);

            for (var i = 0; i < collision.contactCount; i++)
            {
                var normal = collision.GetContact(i).normal;

                if (normal.y >= minGroundDot)
                    continue;

                normal.y = 0f;

                if (normal.sqrMagnitude > 0.001f)
                    _wallNormals.Add(normal.normalized);
            }
        }

        private Vector3 RemoveMovementIntoWalls(Vector3 movement)
        {
            foreach (var normal in _wallNormals)
            {
                var speedIntoWall = Vector3.Dot(movement, normal);

                if (speedIntoWall < 0f)
                    movement -= normal * speedIntoWall;
            }

            _wallNormals.Clear();
            return movement;
        }

        private bool TryGetGroundNormal(out Vector3 normal)
        {
            normal = Vector3.up;

            if (!_playerCollider || !_playerCollider.enabled)
                return false;

            var bounds = _playerCollider.bounds;
            var radius = Mathf.Min(bounds.extents.x, bounds.extents.z) * 0.98f;
            var distance = bounds.extents.y - radius + groundCheckDistance;
            var hitCount = Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down, _groundHits,
                distance, groundLayers, QueryTriggerInteraction.Ignore);
            var closestDistance = float.MaxValue;
            var minGroundDot = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
            var foundGround = false;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _groundHits[i];

                if (hit.rigidbody == rb || hit.distance >= closestDistance)
                    continue;

                var surfaceNormal = hit.normal;
                var contactOrigin = hit.point + Vector3.up * stepClearance -
                                    new Vector3(hit.normal.x, 0f, hit.normal.z) * stepClearance;

                // A sphere hit on a stair edge returns a rounded normal, not the actual tread normal.
                if (hit.collider.Raycast(new Ray(contactOrigin, Vector3.down), out var surface,
                        stepClearance * 2f) && surface.normal.y >= minGroundDot)
                    surfaceNormal = surface.normal;

                if (surfaceNormal.y < minGroundDot)
                    continue;

                closestDistance = hit.distance;
                normal = surfaceNormal;
                foundGround = true;
            }

            return foundGround;
        }
    }
}
