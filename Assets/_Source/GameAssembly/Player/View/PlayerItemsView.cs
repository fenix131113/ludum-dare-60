using System.Collections.Generic;
using GameAssembly.ItemsSystem;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;

namespace GameAssembly.Player.View
{
    public class PlayerItemsView : MonoBehaviour
    {
        [SerializeField] private PlayerItemCell itemCell;
        [SerializeField] private RectTransform itemsPanelRect;
        [SerializeField] private float hideTime;

        [Inject] private PlayerInventory _playerInventory;
        [Inject] private PlayerInput _input;

        private float _hideTimer;

        private readonly List<GameObject> _spawnedItemCells = new();

        private void Start()
        {
            _hideTimer = hideTime;
            Bind();
            Redraw();
        }

        private void Update()
        {
            if (!enabled || !itemsPanelRect.gameObject.activeSelf)
                return;

            _hideTimer -= Time.deltaTime;
            
            if(_hideTimer <= 0)
                HideInventory();
        }

        private void OnInventoryClicked()
        {
            ShowInventory();
        }

        private void OnInventoryChanged()
        {
            Redraw();
            ShowInventory();
        }

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

        private void ShowInventory()
        {
            if (_playerInventory.Items.Count == 0)
                return;
            
            itemsPanelRect.gameObject.SetActive(true);
            _hideTimer = hideTime;
        }
        
        private void HideInventory()
        {
            itemsPanelRect.gameObject.SetActive(false);
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
            _input.OnInventoryPerformed += OnInventoryClicked;
        }

        private void Expose()
        {
            _playerInventory.OnItemAdded -= OnItemAdded;
            _playerInventory.OnItemRemoved -= OnItemRemoved;
            _input.OnInventoryPerformed -= OnInventoryClicked;
        }
    }
}