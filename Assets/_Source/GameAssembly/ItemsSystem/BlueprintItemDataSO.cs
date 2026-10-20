using GameAssembly.ShelterSystem.CraftSystem;
using UnityEngine;

namespace GameAssembly.ItemsSystem
{
    [CreateAssetMenu(fileName = "new BlueprintItemData", menuName = "Game/Blueprint Item Data")]
    public class BlueprintItemDataSO : ItemDataSO
    {
        [field: SerializeField] public CraftItemSO Craft { get; private set; }
        [field: SerializeField] public GameObject LootPrefab { get; private set; }
    }
}
