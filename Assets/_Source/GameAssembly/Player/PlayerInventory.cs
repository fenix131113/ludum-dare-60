using System;
using GameAssembly.InventorySystem;
using GameAssembly.ShelterSystem;
using VContainer;
using VContainer.Unity;

namespace GameAssembly.Player
{
    public class PlayerInventory : Inventory, IInitializable, IDisposable
    {
        [Inject] private PlayerState _playerState;
        [Inject] private ShelterInventory _shelterInventory;

        public void Initialize()
        {
            _playerState.OnShelterStateChanged += OnShelterStateChanged;
        }

        private void OnShelterStateChanged()
        {
            if(!_playerState.IsInShelter())
                return;

            foreach (var item in _items)
                _shelterInventory.AddItem(item.Key, item.Value);
            
            ClearInventory();
        }

        public void Dispose()
        {
            _playerState.OnShelterStateChanged -= OnShelterStateChanged;
        }
    }
}