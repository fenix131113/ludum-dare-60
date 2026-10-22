using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameAssembly.MainMenu
{
    public class GameMenu : MonoBehaviour
    {
        [SerializeField] private ThreeDButton startButton;
        [SerializeField] private int gameSceneIndex;

        private void Start()
        {
            startButton.OnButtonClicked += OnStartClicked;
        }

        private void OnDestroy()
        {
            startButton.OnButtonClicked -= OnStartClicked;
        }

        private void OnStartClicked() => SceneManager.LoadScene(gameSceneIndex);
    }
}