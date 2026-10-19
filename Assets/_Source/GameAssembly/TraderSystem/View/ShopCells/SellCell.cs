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

        public override void Initialize(Sprite icon, string nameText, int count, int itemCost, string buttonLabel)
        {
            base.Initialize(icon, nameText, count, itemCost, buttonLabel);
        }

        public override bool CanUse() => _shelterInventory.IsItemInInventory(sellItem);

        public override void Use()
        {
            if(!CanUse() || !_shelterInventory.TryRemoveItem(sellItem) || _sellMinigame.IsPlaying())
                return;

            _sellMinigame.OnMinigameEnded += OnSellGameEnded;
            _sellMinigame.StartMinigame();
        }

        protected virtual void OnSellGameEnded(bool isWin)
        {
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
            if (_sellMinigame.IsPlaying())
                _sellMinigame.OnMinigameEnded -= OnSellGameEnded;
        }
    }
}