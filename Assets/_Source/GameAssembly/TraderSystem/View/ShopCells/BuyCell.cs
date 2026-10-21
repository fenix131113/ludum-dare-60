using GameAssembly.ItemsSystem;
using GameAssembly.Player;
using GameAssembly.ShelterSystem;
using UnityEngine;
using VContainer;

namespace GameAssembly.TraderSystem.View.ShopCells
{
    public class BuyCell : BaseShopCell
    {
        [SerializeField] private ItemDataSO buyItem;
        [SerializeField, Min(1)] private int count = 1;
        [SerializeField] private string buyButtonText = "Buy";

        [Inject] private PlayerBank _playerBank;
        [Inject] private ShelterInventory _shelterInventory;

        private long TotalCost => (long)buyItem.Cost * count;

        public override void Initialize(ItemDataSO item, int itemCount, string buttonLabel)
        {
            buyItem = item;
            count = itemCount;
            buyButtonText = buttonLabel;
            DrawContent();
        }

        public override bool CanUse() => buyItem && buyItem.Cost >= 0 &&
                                         TotalCost <= int.MaxValue && _playerBank != null &&
                                         _shelterInventory != null && _playerBank.Money >= TotalCost;

        public override void Use()
        {
            if (!CanUse() || !_playerBank.TryRemoveMoney((int)TotalCost))
                return;

            _shelterInventory.AddItem(buyItem, count);
        }

        protected override void Start()
        {
            base.Start();
            _playerBank.OnMoneyChanged += OnMoneyChanged;
            Redraw();
        }

        private void OnMoneyChanged(int oldValue, int newValue) => Redraw();

        private void Redraw()
        {
            DrawContent();
            useButton.interactable = CanUse();
        }

        private void DrawContent()
        {
            if (!buyItem)
            {
                useButton.interactable = false;
                return;
            }

            base.Initialize(buyItem, count, buyButtonText);
            costLabel.text = TotalCost == 0 ? "" : TotalCost + "$";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_playerBank != null)
                _playerBank.OnMoneyChanged -= OnMoneyChanged;
        }

        private void OnValidate()
        {
            count = Mathf.Max(1, count);
        }
    }
}
