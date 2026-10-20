using System;
using GameAssembly.InventorySystem;
using GameAssembly.ItemsSystem;
using GameAssembly.LootLocation.Generation;
using GameAssembly.ShelterSystem;
using GameAssembly.ShelterSystem.CraftSystem;
using VContainer;
using VContainer.Unity;

namespace GameAssembly.Player
{
    public class PlayerInventory : Inventory, IInitializable, IDisposable
    {
        [Inject] private PlayerState _playerState;
        [Inject] private ShelterInventory _shelterInventory;
        [Inject] private CraftView _craftView;
        [Inject] private StoryLootSystem _storyLootSystem;

        public void Initialize()
        {
            _playerState.OnShelterStateChanged += OnShelterStateChanged;
        }

        private void OnShelterStateChanged()
        {
            if(!_playerState.IsInShelter())
                return;

            foreach (var item in _items)
            {
                if (item.Key is BlueprintItemDataSO blueprint)
                {
                    _craftView.UnblockCraft(blueprint.Craft);
                    continue;
                }

                _shelterInventory.AddItem(item.Key, item.Value);
                _storyLootSystem.RegisterCollected(item.Key);
            }
            
            ClearInventory();
        }

        public void Dispose()
        {
            _playerState.OnShelterStateChanged -= OnShelterStateChanged;
        }
    }
}
