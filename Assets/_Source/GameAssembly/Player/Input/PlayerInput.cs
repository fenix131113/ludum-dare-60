using System;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace GameAssembly.Player.Input
{
    public class PlayerInput : IInitializable, IDisposable
    {
        [Inject] private InputSystem_Actions _actions;

        public event Action OnInteractPerformed;
        public event Action OnJumpPerformed;
        
        public void Initialize()
        {
            Bind();
            _actions.Player.Enable();
        }
        
        public void Dispose()
        {
            _actions?.Dispose();
        }

        private void OnInteract(InputAction.CallbackContext callbackContext) => OnInteractPerformed?.Invoke();
        private void OnJump(InputAction.CallbackContext callbackContext) => OnJumpPerformed?.Invoke();

        private void Bind()
        {
            _actions.Player.Interact.performed += OnInteract;
            _actions.Player.Jump.performed += OnJump;
        }
        
        public Vector2 ReadMovementVector() => _actions.Player.Move.ReadValue<Vector2>().normalized;
        public Vector2 ReadLookVector() => _actions.Player.Look.ReadValue<Vector2>();

        public bool IsSprintPressed() => _actions.Player.Sprint.ReadValue<float>() > 0;
    }
}