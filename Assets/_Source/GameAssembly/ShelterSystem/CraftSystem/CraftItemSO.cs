using System.Collections.Generic;
using GameAssembly.ItemsSystem;
using UnityEngine;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    [CreateAssetMenu(fileName = "new CraftItem", menuName = "Game/Craft Item")]
    public class CraftItemSO : ScriptableObject
    {
        [field: SerializeField] public ItemDataSO ItemData { get; private set; }
        [field: SerializeField] public List<CraftRequirement> CraftsRequirements { get; private set; }
    }

    [System.Serializable]
    public class CraftRequirement
    {
        [field: SerializeField] public ItemDataSO ItemData { get; private set; }
        [field: SerializeField] public int Count { get; private set; }
    }
}