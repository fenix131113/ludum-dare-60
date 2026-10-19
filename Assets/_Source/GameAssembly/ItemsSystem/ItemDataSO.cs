using UnityEngine;

namespace GameAssembly.ItemsSystem
{
    [CreateAssetMenu(fileName = "new ItemData", menuName = "Game/Item Data")]
    public class ItemDataSO : ScriptableObject
    {
        [field: SerializeField] public string ItemName { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int Cost { get; private set; }
    }
}