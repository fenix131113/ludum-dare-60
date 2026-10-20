using UnityEngine;

namespace GameAssembly.ShelterSystem
{
    [CreateAssetMenu(fileName = "new DayEvent", menuName = "Game/Day Event")]
    public class DayEventSO : ScriptableObject
    {
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField, TextArea] public string RequirementsDescription { get; private set; }
        [field: SerializeField, Min(0)] public int Cost { get; private set; }
    }
}
