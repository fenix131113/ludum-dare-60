using System;
using GameAssembly.Player.Data;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GameAssembly.Player
{
    public class PlayerState : IInitializable, IDisposable
    {
        public event Action<PlayerInteractStates> OnInteractionStateChanged;
        public event Action OnDimensionChanged;

        [Inject] private PlayerInput _input;
        
        private PlayerInteractStates _currentState = PlayerInteractStates.FREE;
        private bool _isInTwoD;

        public PlayerInteractStates GetCurrentState() => _currentState;
        public bool IsInTwoD() => _isInTwoD;

        public void Initialize()
        {
            _input.OnInteractPerformed += OnInteractPerformed;
        }
        
        public void SwitchDimension(bool isInTwoD)
        {
            SetCurrentInteractionState(isInTwoD ? PlayerInteractStates.LOCKED : PlayerInteractStates.FREE);

            _isInTwoD = isInTwoD;
        }

        public void SetCurrentInteractionState(PlayerInteractStates state)
        {
            if (state == PlayerInteractStates.LOCKED)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            _currentState = state;
            OnInteractionStateChanged?.Invoke(_currentState);
        }
        
        public void Dispose()
        {
            _input.OnInteractPerformed -= OnInteractPerformed;
        }
        
        private void OnInteractPerformed()
        {
            if(IsInTwoD())
                SwitchDimension(false);
        }
    }
}