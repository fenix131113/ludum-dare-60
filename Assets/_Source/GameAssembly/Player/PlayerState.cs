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
        public event Action OnShelterStateChanged;

        [Inject] private PlayerInput _input;
        
        private PlayerInteractStates _currentState = PlayerInteractStates.FREE;
        private bool _isInTwoD;
        private bool _isSecondInteractionBlocked;
        private bool _isInShelter;

        public PlayerInteractStates GetCurrentState() => _currentState;
        public bool IsInTwoD() => _isInTwoD;
        public bool IsSecondInteractionBlocked() => _isSecondInteractionBlocked;
        public bool IsInShelter() => _isInShelter;

        public void Initialize()
        {
            _input.OnInteractPerformed += OnInteractPerformed;
        }

        public void SwitchShelterState(bool inShelter)
        {
            _isInShelter = inShelter;
            OnShelterStateChanged?.Invoke();
        }
        
        public void ChangeBlockSecondInteraction(bool blocked) => _isSecondInteractionBlocked = blocked;

        public void SwitchDimension(bool isInTwoD)
        {
            SetCurrentInteractionState(isInTwoD ? PlayerInteractStates.LOCKED : PlayerInteractStates.FREE);

            _isInTwoD = isInTwoD;
            OnDimensionChanged?.Invoke();
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
            if(IsInTwoD() && !_isSecondInteractionBlocked)
                SwitchDimension(false);
        }
    }
}