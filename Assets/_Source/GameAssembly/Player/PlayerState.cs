using System;
using GameAssembly.Player.Data;
using UnityEngine;

namespace GameAssembly.Player
{
    public class PlayerState
    {
        public event Action<PlayerInteractStates> OnStateChanged;
        
        private PlayerInteractStates _currentState = PlayerInteractStates.FREE;
        
        public PlayerInteractStates GetCurrentState() => _currentState;

        public void SetCurrentState(PlayerInteractStates state)
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
            OnStateChanged?.Invoke(_currentState);
        }
    }
}