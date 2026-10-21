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

        private readonly RaycastHit[] _groundHits = new RaycastHit[16];
        private readonly List<Vector3> _wallNormals = new();
        private Collider _playerCollider;

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
            worldMove = RemoveMovementIntoWalls(worldMove);
            _playerStamina.Tick(isSprinting && worldMove.sqrMagnitude > 0.001f, Time.fixedDeltaTime);
            if (TryGetGroundNormal(out var groundNormal))
            {
                var groundMove = Vector3.ProjectOnPlane(worldMove, groundNormal).normalized * worldMove.magnitude;
                rb.linearVelocity = groundMove - groundNormal * groundStickSpeed;
            }
            else
                rb.linearVelocity = new Vector3(worldMove.x, Mathf.Min(rb.linearVelocity.y, 0f), worldMove.z);
        }

        private void OnCollisionEnter(Collision collision) => RegisterWallContacts(collision);
        private void OnCollisionStay(Collision collision) => RegisterWallContacts(collision);

        private void OnDisable() => _wallNormals.Clear();

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
            var radius = Mathf.Min(bounds.extents.x, bounds.extents.z) * 0.9f;
            var distance = bounds.extents.y - radius + groundCheckDistance;
            var hitCount = Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down, _groundHits,
                distance, groundLayers, QueryTriggerInteraction.Ignore);
            var closestDistance = float.MaxValue;
            var minGroundDot = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
            var foundGround = false;

            for (var i = 0; i < hitCount; i++)
            {
                var hit = _groundHits[i];

                if (hit.rigidbody == rb || hit.normal.y < minGroundDot || hit.distance >= closestDistance)
                    continue;

                closestDistance = hit.distance;
                normal = hit.normal;
                foundGround = true;
            }

            return foundGround;
        }
    }
}
