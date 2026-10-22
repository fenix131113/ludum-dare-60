using GameAssembly.Player;
using GameAssembly.Player.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace GameAssembly.Core
{
    public class GameMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject menuPanel;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameSettingsView settingsView;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button backButton;
        [SerializeField] private Button quitButton;

        [Inject] private PlayerInput _input;
        [Inject] private PlayerState _playerState;
        [Inject] private ScreenTransition _screenTransition;

        private bool _isOpen;

        private void Awake()
        {
            if (menuPanel)
                menuPanel.SetActive(false);

            mainPanel.SetActive(true);
            settingsPanel.SetActive(false);

#if UNITY_WEBGL
            quitButton.gameObject.SetActive(false);
#endif
        }

        private void Start()
        {
            _input.OnMenuPerformed += Toggle;
            continueButton.onClick.AddListener(Close);
            settingsButton.onClick.AddListener(OpenSettings);
            backButton.onClick.AddListener(ShowMain);
            quitButton.onClick.AddListener(Quit);
        }

        private void Toggle()
        {
            if (_isOpen)
                Close();
            else
                Open();
        }

        public void Open()
        {
            if (_isOpen || _screenTransition.IsTransitioning || _playerState.IsGameEnded())
                return;

            _isOpen = true;
            _playerState.SetMenuOpen(true);
            menuPanel.transform.SetAsLastSibling();
            menuPanel.SetActive(true);
            ShowMain();
        }

        private void OpenSettings()
        {
            mainPanel.SetActive(false);
            settingsPanel.SetActive(true);
            backButton.Select();
        }

        private void ShowMain()
        {
            settingsView.Save();
            settingsPanel.SetActive(false);
            mainPanel.SetActive(true);
            continueButton.Select();
        }

        public void Close()
        {
            if (!_isOpen)
                return;

            _isOpen = false;
            settingsView.Save();
            menuPanel.SetActive(false);
            _playerState.SetMenuOpen(false);

            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void Quit()
        {
#if UNITY_WEBGL
            return;
#elif UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDisable() => Close();

        private void OnDestroy()
        {
            if (_input != null)
                _input.OnMenuPerformed -= Toggle;

            if (continueButton)
                continueButton.onClick.RemoveListener(Close);

            if (settingsButton)
                settingsButton.onClick.RemoveListener(OpenSettings);

            if (backButton)
                backButton.onClick.RemoveListener(ShowMain);

            if (quitButton)
                quitButton.onClick.RemoveListener(Quit);
        }
    }
}
