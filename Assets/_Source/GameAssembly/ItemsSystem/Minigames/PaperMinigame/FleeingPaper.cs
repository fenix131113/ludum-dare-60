using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameAssembly.ItemsSystem.Minigames.PaperMinigame
{
    public class FleeingPaper : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRectTransform;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private float triggerRadius = 150f;
        [SerializeField] private float fleeForce = 800f;
        [SerializeField] private float drag = 1.5f;
        [SerializeField] private float windStrength = 80f;
        [SerializeField] private float windSpeed = 3f;
        [SerializeField] private int dodgesBeforeRest = 4;
        [SerializeField] private float restDuration = 2.5f;

        public event Action OnPaperCaught;

        private Vector2 _canvasSize;
        private Vector2 _velocity;
        private Vector2 _windDirection;

        private int _currentDodges;
        private float _restTimer;
        private float _windTimeOffset;

        private bool _isResting;
        private bool _wasNearMouseLastFrame;

        public void ResetPaperState()
        {
            _velocity = Vector2.zero;
            _windDirection = Vector2.zero;

            _currentDodges = 0;
            _wasNearMouseLastFrame = false;

            _isResting = false;
            _restTimer = 0f;
            
            rectTransform.rotation = Quaternion.identity;
        }
        
        private void Start()
        {
            _canvasSize = canvasRectTransform.rect.size;
            _windTimeOffset = UnityEngine.Random.Range(0f, 100f);
        }

        private void Update()
        {
            if (Mouse.current == null) return;

            if (_isResting)
            {
                _velocity = Vector2.zero;
                _restTimer -= Time.deltaTime;

                if (_restTimer <= 0)
                {
                    _isResting = false;
                    _currentDodges = 0;
                }

                CheckClickAttempt();
                return;
            }

            HandleMouseInfluence();

            ApplyWindMovement();

            _velocity -= _velocity * (drag * Time.deltaTime);

            rectTransform.anchoredPosition += _velocity * Time.deltaTime;

            HandleScreenBounds();

            ApplyVisualRotation();
        }

        private void HandleMouseInfluence()
        {
            var mouseScreenPos = Mouse.current.position.ReadValue();

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, mouseScreenPos, null,
                    out var localMousePos))
                return;

            var distanceToMouse = Vector2.Distance(rectTransform.anchoredPosition, localMousePos);

            if (distanceToMouse < triggerRadius)
            {
                var pushDirection = (rectTransform.anchoredPosition - localMousePos).normalized;

                if (pushDirection == Vector2.zero)
                    pushDirection = UnityEngine.Random.insideUnitCircle.normalized;

                var proximityFactor = 1f - (distanceToMouse / triggerRadius);
                _velocity += pushDirection * fleeForce * proximityFactor * Time.deltaTime * 60f;

                if (_wasNearMouseLastFrame)
                    return;

                _wasNearMouseLastFrame = true;
                _currentDodges++;

                if (_currentDodges < dodgesBeforeRest)
                    return;

                _isResting = true;
                _restTimer = restDuration;
            }

            _wasNearMouseLastFrame = false;
        }

        private void ApplyWindMovement()
        {
            var time = Time.time * windSpeed + _windTimeOffset;
            _windDirection.x = Mathf.Sin(time) * Mathf.Cos(time * 0.6f);
            _windDirection.y = Mathf.Cos(time * 0.8f) * Mathf.Sin(time * 0.5f);

            _velocity += _windDirection * windStrength * Time.deltaTime * 60f;
        }

        private void HandleScreenBounds()
        {
            const float padding = 50f;
            var halfWidth = (_canvasSize.x / 2f) - padding;
            var halfHeight = (_canvasSize.y / 2f) - padding;

            var pos = rectTransform.anchoredPosition;

            if (pos.x < -halfWidth)
            {
                pos.x = -halfWidth;
                _velocity.x = Mathf.Abs(_velocity.x) * 0.8f;
            }
            else if (pos.x > halfWidth)
            {
                pos.x = halfWidth;
                _velocity.x = -Mathf.Abs(_velocity.x) * 0.8f;
            }

            if (pos.y < -halfHeight)
            {
                pos.y = -halfHeight;
                _velocity.y = Mathf.Abs(_velocity.y) * 0.8f;
            }
            else if (pos.y > halfHeight)
            {
                pos.y = halfHeight;
                _velocity.y = -Mathf.Abs(_velocity.y) * 0.8f;
            }

            rectTransform.anchoredPosition = pos;
        }

        private void ApplyVisualRotation()
        {
            if (_velocity.magnitude <= 10f)
                return;

            var targetAngle = Mathf.Atan2(_velocity.y, _velocity.x) * Mathf.Rad2Deg - 90f;

            var targetRotation = Quaternion.Euler(0, 0, targetAngle);
            rectTransform.rotation = Quaternion.Lerp(rectTransform.rotation, targetRotation, Time.deltaTime * 5f);
        }

        private void CheckClickAttempt()
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame)
                return;

            var mouseScreenPos = Mouse.current.position.ReadValue();

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mouseScreenPos, null))
                CatchPaper();
        }

        private void CatchPaper()
        {
            OnPaperCaught?.Invoke();
            gameObject.SetActive(false);
        }
    }
}