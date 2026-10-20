using System.Collections.Generic;
using System.Linq;
using GameAssembly.ItemsSystem;
using GameAssembly.ShelterSystem;
using GameAssembly.ShelterSystem.CraftSystem;
using VContainer;

namespace GameAssembly.LootLocation.Generation
{
    public class StoryLootSystem
    {
        [Inject] private CraftView _craftView;
        [Inject] private ShelterInventory _shelterInventory;

        private readonly HashSet<ItemDataSO> _collectedItems = new();
        private readonly HashSet<ItemDataSO> _spawnedItems = new();

        public void BeginLocation() => _spawnedItems.Clear();

        public void RegisterSpawned(ItemDataSO item) => _spawnedItems.Add(item);

        public void RegisterCollected(ItemDataSO item)
        {
            if (item && item.StoryItem)
                _collectedItems.Add(item);
        }

        public bool CanSpawn(ItemDataSO item)
        {
            if (!item || !item.StoryItem || _collectedItems.Contains(item) || _spawnedItems.Contains(item))
                return false;

            if (_shelterInventory.IsItemInInventory(item))
            {
                RegisterCollected(item);
                return false;
            }

            return _craftView.Crafts.Any(craft => craft && craft.ItemData && craft.ItemData.StoryItem &&
                !_craftView.IsCraftBlocked(craft) && craft.CraftsRequirements != null &&
                craft.CraftsRequirements.Any(requirement => requirement != null && requirement.Count > 0 && requirement.ItemData == item));
        }
    }
}
