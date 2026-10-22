using System;
using System.Collections.Generic;
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
        private bool _isInShelter = true;
        private bool _isMenuOpen;
        private bool _isGameEnded;
        private readonly HashSet<object> _controlLocks = new();

        public PlayerInteractStates GetCurrentState() =>
            _isMenuOpen || _isInTwoD || _controlLocks.Count > 0 || _isGameEnded
                ? PlayerInteractStates.LOCKED
                : _currentState;

        public bool IsMenuOpen() => _isMenuOpen;
        public bool IsInTwoD() => _isInTwoD;
        public bool IsGameEnded() => _isGameEnded;

        public bool IsSecondInteractionBlocked() =>
            _isSecondInteractionBlocked || _isMenuOpen || _controlLocks.Count > 0 || _isGameEnded;

        public bool IsInShelter() => _isInShelter;

        public void Initialize()
        {
            _input.OnInteractPerformed += OnInteractPerformed;
        }

        public void SetGameEnded(bool isGameEnded) => _isGameEnded = isGameEnded;

        public void SwitchShelterState(bool inShelter)
        {
            _isInShelter = inShelter;
            OnShelterStateChanged?.Invoke();
        }

        public void ChangeBlockSecondInteraction(bool blocked) => _isSecondInteractionBlocked = blocked;

        public void SwitchDimension(bool isInTwoD)
        {
            if (_isInTwoD == isInTwoD)
                return;

            _isInTwoD = isInTwoD;
            ApplyInteractionState();
            OnDimensionChanged?.Invoke();
        }

        public void SetMenuOpen(bool isOpen)
        {
            _isMenuOpen = isOpen;
            _input?.SetGameplayInputBlocked(isOpen);
            ApplyInteractionState();
        }

        public void LockControls(object owner)
        {
            if (owner != null && _controlLocks.Add(owner))
                ApplyInteractionState();
        }

        public void UnlockControls(object owner)
        {
            if (owner != null && _controlLocks.Remove(owner))
                ApplyInteractionState();
        }

        public void SetCurrentInteractionState(PlayerInteractStates state)
        {
            _currentState = state;
            ApplyInteractionState();
        }

        private void ApplyInteractionState()
        {
            var state = GetCurrentState();

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

            OnInteractionStateChanged?.Invoke(state);
        }

        public void Dispose()
        {
            _input.OnInteractPerformed -= OnInteractPerformed;
        }

        private void OnInteractPerformed()
        {
            if (IsInTwoD() && !IsSecondInteractionBlocked())
                SwitchDimension(false);
        }
    }
}