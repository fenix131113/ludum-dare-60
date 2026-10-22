using GameAssembly.Player;
using UnityEngine;
using VContainer;

namespace GameAssembly.Core.View
{
    public class EndGameView : MonoBehaviour
    {
        [SerializeField] private GameObject winGamePanel;
        
        [Inject] private PlayerState _playerState;
        
        public void WinGame()
        {
            winGamePanel.SetActive(true);
            _playerState.SetGameEnded(true);
        }
    }
}