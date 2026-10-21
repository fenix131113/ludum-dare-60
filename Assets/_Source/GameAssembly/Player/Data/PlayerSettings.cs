using UnityEngine;

namespace GameAssembly.Player.Data
{
    [CreateAssetMenu(fileName = "new PlayerSettings", menuName = "Game/PlayerSettings")]
    public class PlayerSettings : ScriptableObject
    {
        [field: SerializeField] public float Speed { get; private set; }
        [field: SerializeField] public float SprintSpeed { get; private set; }
        [field: SerializeField] public float RayLenght { get; private set; }
        [field: SerializeField] public float TwoDSpeed { get; private set; }
        [field: SerializeField, Range(0f, 1f)] public float ShelterSpeedMultiplier { get; private set; } = 0.65f;
        [field: SerializeField, Min(0.01f)] public float SprintDuration { get; private set; } = 5f;
        [field: SerializeField, Min(0.01f)] public float StaminaRecoveryDuration { get; private set; } = 4f;
        [field: SerializeField, Min(0f)] public float StaminaRecoveryDelay { get; private set; } = 1f;
        [field: SerializeField, Range(0.01f, 1f)] public float SprintResumeThreshold { get; private set; } = 0.25f;
    }
}
