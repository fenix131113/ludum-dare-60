using UnityEngine;

namespace GameAssembly.ItemsSystem
{
    [CreateAssetMenu(fileName = "new ItemData", menuName = "Game/Item Data")]
    public class ItemDataSO : ScriptableObject
    {
        [field: SerializeField] public string ItemName { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int Cost { get; private set; }
        [field: SerializeField] public bool StoryItem { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField] public GameObject LootPrefab { get; private set; }
    }
}
