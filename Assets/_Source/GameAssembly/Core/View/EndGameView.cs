using GameAssembly.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

namespace GameAssembly.Core.View
{
    public class EndGameView : MonoBehaviour
    {
        [SerializeField] private GameObject winGamePanel;
        [SerializeField] private Button quitToMenuButton;

        [Inject] private PlayerState _playerState;

        public void WinGame()
        {
            winGamePanel.SetActive(true);
            _playerState.SetGameEnded(true);
        }

        private void Start() => quitToMenuButton.onClick.AddListener(OnExitButtonClicked);

        private void OnDestroy() => quitToMenuButton.onClick.RemoveAllListeners();

        private void OnExitButtonClicked() => SceneManager.LoadScene(0);
    }
}