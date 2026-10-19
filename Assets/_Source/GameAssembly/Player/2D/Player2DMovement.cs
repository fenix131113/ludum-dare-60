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

        public void Teleport(Vector3 position) => transform.position = position;

        private void FixedUpdate()
        {
            if (!_playerState.IsInTwoD() || _playerState.IsSecondInteractionBlocked())
                return;

            var moveVector = _playerInput.ReadMovementVector();

            var rawX = moveVector.x switch
            {
                > 0.1f => 1f,
                < -0.1f => -1f,
                _ => 0f
            };
            
            var localVelocity = new Vector3(rawX * _playerSettings.TwoDSpeed, 0, 0);
            var globalMoveDirection = rb.rotation * localVelocity;
            rb.linearVelocity = new Vector3(globalMoveDirection.x, rb.linearVelocity.y, globalMoveDirection.z);
        }

    }
}