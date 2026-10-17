using System.Collections.Generic;
using GameAssembly.ItemsSystem;
using UnityEngine;
using VContainer;

namespace GameAssembly.Player.View
{
    public class PlayerItemsView : MonoBehaviour
    {
        [SerializeField] private PlayerItemCell itemCell;
        [SerializeField] private RectTransform itemsPanelRect;

        [Inject] private PlayerInventory _playerInventory;

        private readonly List<GameObject> _spawnedItemCells = new();

        private void Start()
        {
            Bind();
            Redraw();
        }

        private void OnInventoryChanged() => Redraw();

        private void Redraw()
        {
            if (_playerInventory.Items.Count == 0)
            {
                itemsPanelRect.gameObject.SetActive(false);
                return;
            }

            itemsPanelRect.gameObject.SetActive(true);

            _spawnedItemCells.ForEach(Destroy);
            _spawnedItemCells.Clear();

            foreach (var item in _playerInventory.Items)
            {
                var spawned = Instantiate(itemCell, Vector2.zero, Quaternion.identity, itemsPanelRect);
                spawned.Initialize(item.Key.Icon, item.Value, item.Key.ItemName);

                _spawnedItemCells.Add(spawned.gameObject);
            }
        }

        private void OnItemAdded(ItemDataSO item, int count) => OnInventoryChanged();
        private void OnItemRemoved(ItemDataSO item, int count) => OnInventoryChanged();

        private void OnDestroy()
        {
            Expose();
        }

        private void Bind()
        {
            _playerInventory.OnItemAdded += OnItemAdded;
            _playerInventory.OnItemRemoved += OnItemRemoved;
        }

        private void Expose()
        {
            _playerInventory.OnItemAdded -= OnItemAdded;
            _playerInventory.OnItemRemoved -= OnItemRemoved;
        }
    }
}