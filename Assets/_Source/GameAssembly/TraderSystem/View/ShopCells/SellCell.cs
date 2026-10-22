using GameAssembly.ItemsSystem;
using GameAssembly.Player;
using GameAssembly.ShelterSystem;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace GameAssembly.TraderSystem.View.ShopCells
{
    public class SellCell : BaseShopCell
    {
        [SerializeField] private Button negotiateButton;
        [SerializeField] private ItemDataSO sellItem;

        [Inject] protected PlayerBank _playerBank;
        [Inject] protected ShelterInventory _shelterInventory;
        [Inject] protected SellMinigame _sellMinigame;

        private bool _isSalePending;

        public override void Initialize(ItemDataSO item, int count, string buttonLabel)
        {
            sellItem = item;
            negotiateButton.onClick.AddListener(NegotiateButtonClicked);
            base.Initialize(item, count, buttonLabel);
        }

        public override bool CanUse() => sellItem && !sellItem.StoryItem && !_isSalePending &&
                                         !_sellMinigame.IsPlaying() &&
                                         _shelterInventory.IsItemInInventory(sellItem);

        public override void Use()
        {
            if (!CanUse() || !_shelterInventory.TryRemoveItem(sellItem))
                return;

            _playerBank.AddMoney(sellItem.Cost);

            if (_shelterInventory.IsItemInInventory(sellItem))
            {
                countLabel.text = "x" + _shelterInventory.Items[sellItem].ToString();
                return;
            }

            Destroy(gameObject);
        }

        protected void NegotiateButtonClicked()
        {
            if (!CanUse() || !_shelterInventory.TryRemoveItem(sellItem))
                return;

            _isSalePending = true;
            _sellMinigame.OnMinigameEnded += OnSellGameEnded;
            _sellMinigame.StartMinigame();
        }

        protected virtual void OnSellGameEnded(bool isWin)
        {
            if (!_isSalePending)
                return;

            _isSalePending = false;
            _sellMinigame.OnMinigameEnded -= OnSellGameEnded;

            if (_sellMinigame.IsDefaultPrice)
                _playerBank.AddMoney(sellItem.Cost);
            else
                _playerBank.AddMoney(isWin
                    ? Mathf.RoundToInt(sellItem.Cost * 1.2f)
                    : Mathf.RoundToInt(sellItem.Cost * 0.8f));

            if (!_shelterInventory.IsItemInInventory(sellItem))
            {
                Destroy(gameObject);
                return;
            }

            countLabel.text = "x" + _shelterInventory.Items[sellItem].ToString();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            negotiateButton.onClick.RemoveAllListeners();
            if (_sellMinigame)
                _sellMinigame.OnMinigameEnded -= OnSellGameEnded;

            if (_isSalePending)
            {
                _isSalePending = false;
                _shelterInventory.AddItem(sellItem);
            }
        }
    }
}