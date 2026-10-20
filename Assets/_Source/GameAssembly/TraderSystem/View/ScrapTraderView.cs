using System.Collections.Generic;
using GameAssembly.ItemsSystem;
using GameAssembly.ShelterSystem;
using GameAssembly.TraderSystem.View.ShopCells;
using UnityEngine;
using VContainer;

namespace GameAssembly.TraderSystem.View
{
    public class ScrapTraderView : TraderView
    {
        [SerializeField] private List<ItemDataSO> allowedToSellItems;
        [SerializeField] private AShopCell cellShopPrefab;
        [SerializeField] private Transform sellContent;
            
        [Inject] private ShelterInventory _shelterInventory;
        
        private readonly List<AShopCell> _shopCells = new();
        
        public override void OpenTraderView()
        {
            base.OpenTraderView();
            
            foreach (var item in _shelterInventory.Items)
            {
                if(!allowedToSellItems.Contains(item.Key))
                    continue;
                
                var spawned = Instantiate(cellShopPrefab, sellContent);
                _shopCells.Add(spawned);
                spawned.Initialize(item.Key, item.Value, "Sell");
            }
        }

        public override void CloseTraderView()
        {
            base.CloseTraderView();
            _shopCells.ForEach(x => Destroy(x.gameObject));
            _shopCells.Clear();
        }
    }
}
