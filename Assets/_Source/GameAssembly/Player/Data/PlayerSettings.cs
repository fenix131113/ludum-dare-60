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
    }
}