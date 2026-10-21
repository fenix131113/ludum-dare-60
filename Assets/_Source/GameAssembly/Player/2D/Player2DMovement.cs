using GameAssembly.Player.Data;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;

namespace GameAssembly.Player._2D
{
    public class Player2DMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private PlayerState _playerState;
        [Inject] private PlayerInput _playerInput;

        private TradingZoneScroll _scrollZone;

        public void Teleport(Vector3 position)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
            transform.position = position;
        }

        public void SetScrollZone(TradingZoneScroll scrollZone)
        {
            _scrollZone = scrollZone;

            if (_scrollZone)
                _scrollZone.ResetScroll();
        }

        private void Start() => _playerState.OnDimensionChanged += OnDimensionChanged;

        private void OnDimensionChanged()
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (!_playerState.IsInTwoD())
            {
                if (_scrollZone)
                    _scrollZone.ResetScroll();

                _scrollZone = null;
            }
        }

        private void OnDestroy()
        {
            if (_playerState != null)
                _playerState.OnDimensionChanged -= OnDimensionChanged;
        }

        private void FixedUpdate()
        {
            if (!_playerState.IsInTwoD())
                return;

            if (_playerState.IsSecondInteractionBlocked())
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                return;
            }

            var moveVector = _playerInput.ReadMovementVector();

            var rawX = moveVector.x switch
            {
                > 0.1f => 1f,
                < -0.1f => -1f,
                _ => 0f
            };
            
            var localVelocity = new Vector3(rawX * _playerSettings.TwoDSpeed, 0, 0);
            var globalMoveDirection = rb.rotation * localVelocity;

            if (_scrollZone && _scrollZone.TryGetVelocity(transform, rb.position, localVelocity.x,
                    Time.fixedDeltaTime, out var scrollVelocity))
                globalMoveDirection = scrollVelocity;

            rb.linearVelocity = new Vector3(globalMoveDirection.x, rb.linearVelocity.y, globalMoveDirection.z);
        }

    }
}
