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

        private bool _gameplayInputBlocked;

        public void SetGameplayInputBlocked(bool blocked) => _gameplayInputBlocked = blocked;

        public event Action OnInteractPerformed;
        public event Action OnMenuPerformed;
        public event Action OnSecondInteractPerformed;
        public event Action OnJumpPerformed;
        public event Action OnInventoryPerformed;
        
        public void Initialize()
        {
            _actions.Player.Enable();
            Bind();
        }
        
        public void Dispose()
        {
            _actions.Player.Jump.performed -= OnJump;
            _actions.Player.Interact.performed -= OnInteract;
            _actions.Player.SecondInteract.performed -= OnSecondInteract;
            _actions.Player.Inventory.performed -= OnInventory;
            _actions.Player.Menu.performed -= OnMenu;
            _actions?.Dispose();
        }

        private void OnInteract(InputAction.CallbackContext callbackContext)
        {
            if (!_gameplayInputBlocked)
                OnInteractPerformed?.Invoke();
        }
        private void OnSecondInteract(InputAction.CallbackContext callbackContext)
        {
            if (!_gameplayInputBlocked)
                OnSecondInteractPerformed?.Invoke();
        }

        private void OnJump(InputAction.CallbackContext callbackContext)
        {
            if (!_gameplayInputBlocked)
                OnJumpPerformed?.Invoke();
        }
        private void OnInventory(InputAction.CallbackContext callbackContext)
        {
            if (!_gameplayInputBlocked)
                OnInventoryPerformed?.Invoke();
        }
        private void OnMenu(InputAction.CallbackContext callbackContext) => OnMenuPerformed?.Invoke();

        private void Bind()
        {
            _actions.Player.Inventory.performed += OnInventory;
            _actions.Player.Interact.performed += OnInteract;
            _actions.Player.SecondInteract.performed += OnSecondInteract;
            _actions.Player.Jump.performed += OnJump;
            _actions.Player.Menu.performed += OnMenu;
        }
        
        public Vector2 ReadMovementVector() => _gameplayInputBlocked ? Vector2.zero : _actions.Player.Move.ReadValue<Vector2>().normalized;
        public Vector2 ReadLookVector() => _gameplayInputBlocked ? Vector2.zero : _actions.Player.Look.ReadValue<Vector2>();
        public Vector2 ReadScrollVector() => _gameplayInputBlocked ? Vector2.zero : _actions.Player.Scroll.ReadValue<Vector2>();

        public bool IsSprintPressed() => !_gameplayInputBlocked && _actions.Player.Sprint.ReadValue<float>() > 0;
    }
}