using GameAssembly.Player.Data;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;

namespace GameAssembly.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private PlayerCamera playerCamera;

        [Inject] private PlayerSettings _playerSettings;
        [Inject] private PlayerInput _playerInput;

        private void FixedUpdate()
        {
            var currentSpeed = _playerSettings.Speed;
            
            if(_playerInput.IsSprintPressed())
                currentSpeed = _playerSettings.SprintSpeed;
            
            var moveVector = _playerInput.ReadMovementVector() * currentSpeed;

            var cameraYRotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
            var worldMove = cameraYRotation * new Vector3(moveVector.x, 0f, moveVector.y);
            rb.linearVelocity = new Vector3(worldMove.x, rb.linearVelocity.y, worldMove.z);
        }
    }
}