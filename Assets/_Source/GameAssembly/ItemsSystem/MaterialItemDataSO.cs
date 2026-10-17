using UnityEngine;

namespace GameAssembly.ItemsSystem
{
    [CreateAssetMenu(fileName = "new MaterialItemData", menuName = "Game/Material Item Data")]
    public class MaterialItemDataSO : ItemDataSO
    {
        [field: SerializeField] public GameObject LootPrefab { get; private set; }
        [field: SerializeField] public GameObject WorkbenchPrefab { get; private set; }
    }
}