using GameAssembly.Player.Data;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;

namespace GameAssembly.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;

        [Inject] private PlayerCamera _playerCamera;
        [Inject] private PlayerSettings _playerSettings;
        [Inject] private PlayerState _playerState;
        [Inject] private PlayerInput _playerInput;

        public void Teleport(Vector3 position) => transform.position = position;
        
        private void FixedUpdate()
        {
            if((_playerState.GetCurrentState() != PlayerInteractStates.FREE &&
                _playerState.GetCurrentState() != PlayerInteractStates.MOVEMENT_ONLY) || _playerState.IsInTwoD())
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                return;
            }

            var currentSpeed = _playerSettings.Speed;
            
            if(_playerInput.IsSprintPressed())
                currentSpeed = _playerSettings.SprintSpeed;
            
            var moveVector = _playerInput.ReadMovementVector() * currentSpeed;

            var cameraYRotation = Quaternion.Euler(0f, _playerCamera.transform.eulerAngles.y, 0f);
            var worldMove = cameraYRotation * new Vector3(moveVector.x, 0f, moveVector.y);
            rb.linearVelocity = new Vector3(worldMove.x, rb.linearVelocity.y, worldMove.z);
        }
    }
}