using GameAssembly.Player;
using GameAssembly.TraderSystem.View;
using UnityEngine;

namespace GameAssembly.TraderSystem
{
    public class Trader2D : MonoBehaviour, IInteractable
    {
        [SerializeField] private TraderView traderView;

        
        public bool IsInteractable() => true;

        public void Interact(object sender)
        {
            traderView.OpenTraderView();
        }
    }
}