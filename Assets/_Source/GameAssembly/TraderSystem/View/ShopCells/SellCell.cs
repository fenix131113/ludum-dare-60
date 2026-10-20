using GameAssembly.ItemsSystem;
using GameAssembly.Player;
using GameAssembly.ShelterSystem;
using UnityEngine;
using VContainer;

namespace GameAssembly.TraderSystem.View.ShopCells
{
    public class SellCell : BaseShopCell
    {
        [SerializeField] private ItemDataSO sellItem;

        [Inject] protected PlayerBank _playerBank;
        [Inject] protected ShelterInventory _shelterInventory;
        [Inject] protected SellMinigame _sellMinigame;

        private bool _isSalePending;

        public override void Initialize(ItemDataSO item, int count, string buttonLabel)
        {
            sellItem = item;
            base.Initialize(item, count, buttonLabel);
        }

        public override bool CanUse() => sellItem && !sellItem.StoryItem && !_isSalePending && !_sellMinigame.IsPlaying() &&
                                         _shelterInventory.IsItemInInventory(sellItem);

        public override void Use()
        {
            if(!CanUse() || !_shelterInventory.TryRemoveItem(sellItem))
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

            if(_sellMinigame.IsDefaultPrice)
                _playerBank.AddMoney(sellItem.Cost);
            else
                _playerBank.AddMoney(isWin ? Mathf.RoundToInt(sellItem.Cost * 1.2f) : Mathf.RoundToInt(sellItem.Cost * 0.8f));
            
            if(!_shelterInventory.IsItemInInventory(sellItem))
            {
                Destroy(gameObject);
                return;
            }
            
            countLabel.text = "x" + _shelterInventory.Items[sellItem].ToString();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_sellMinigame != null)
                _sellMinigame.OnMinigameEnded -= OnSellGameEnded;

            if (_isSalePending)
            {
                _isSalePending = false;
                _shelterInventory.AddItem(sellItem);
            }
        }
    }
}
