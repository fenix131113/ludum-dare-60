using System;
using System.Collections.Generic;
using GameAssembly.ItemsSystem;

namespace GameAssembly.InventorySystem
{
    public class Inventory
    {
        public IReadOnlyDictionary<ItemDataSO, int> Items => _items;
        
        public event Action<ItemDataSO, int> OnItemAdded;
        public event Action<ItemDataSO, int> OnItemRemoved;

        protected readonly Dictionary<ItemDataSO, int> _items = new();
        
        protected virtual void ItemAdded(ItemDataSO item, int count) => OnItemAdded?.Invoke(item, count);
        protected virtual void ItemRemoved(ItemDataSO item, int count) => OnItemRemoved?.Invoke(item, count);
        
        public virtual void AddItem(ItemDataSO item, int count = 1)
        {
            if (!_items.TryAdd(item, count))
            {
                _items[item] += count;
            }
            
            ItemAdded(item, count);
        }

        public virtual bool TryRemoveItem(ItemDataSO item, int count = 1)
        {
            if (!_items.ContainsKey(item) || _items[item] < count)
                return false;

            _items[item] -= count;

            if (_items[item] <= 0)
            {
                _items.Remove(item);
                ItemRemoved(item, count);
            }

            return true;
        }

        public virtual bool IsItemInInventory(ItemDataSO item, int count = 1) =>
            _items.ContainsKey(item) && _items[item] >= count;
    }
}