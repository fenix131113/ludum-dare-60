using UnityEngine;

namespace GameAssembly.TraderSystem.View.ShopCells
{
    public abstract class AShopCell : MonoBehaviour
    {
        public abstract void Initialize(Sprite icon, string nameText, int count, int cost, string buttonLabel);
        public abstract bool CanUse();
        public abstract void Use();
    }
}