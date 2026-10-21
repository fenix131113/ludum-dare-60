using GameAssembly.Player.Data;
using UnityEngine;

namespace GameAssembly.Player
{
    public class PlayerStamina
    {
        private readonly PlayerSettings _playerSettings;
        private float _recoveryDelay;
        private bool _isExhausted;

        public float Value { get; private set; } = 1f;
        public bool CanSprint => Value > 0f && !_isExhausted;

        public PlayerStamina(PlayerSettings playerSettings) => _playerSettings = playerSettings;

        public void Tick(bool isSprinting, float deltaTime)
        {
            if (isSprinting && CanSprint)
            {
                Value = Mathf.Max(0f, Value - deltaTime / Mathf.Max(0.01f, _playerSettings.SprintDuration));
                _recoveryDelay = _playerSettings.StaminaRecoveryDelay;

                if (Value <= 0f)
                    _isExhausted = true;

                return;
            }

            var recoveryTime = Mathf.Max(0f, deltaTime - _recoveryDelay);
            _recoveryDelay = Mathf.Max(0f, _recoveryDelay - deltaTime);
            Value = Mathf.MoveTowards(Value, 1f,
                recoveryTime / Mathf.Max(0.01f, _playerSettings.StaminaRecoveryDuration));

            if (_isExhausted && Value >= _playerSettings.SprintResumeThreshold)
                _isExhausted = false;
        }
    }
}
