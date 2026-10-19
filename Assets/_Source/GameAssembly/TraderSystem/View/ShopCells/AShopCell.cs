using UnityEngine;

using GameAssembly.ItemsSystem;

namespace GameAssembly.TraderSystem.View.ShopCells
{
    public abstract class AShopCell : MonoBehaviour
    {
        public abstract void Initialize(ItemDataSO item, int count, string buttonLabel);
        public abstract bool CanUse();
        public abstract void Use();
    }
}
