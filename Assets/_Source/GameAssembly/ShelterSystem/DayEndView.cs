using System;
using System.Collections;
using System.Collections.Generic;
using GameAssembly.Player;
using GameAssembly.Player.Data;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;
using Random = UnityEngine.Random;

namespace GameAssembly.ShelterSystem
{
    public class DayEndView : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private List<DayEventSO> events = new();
        [SerializeField, Min(0)] private int peacefulRaidsCount = 2;

        [Header("Event UI")]
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text descriptionLabel;
        [SerializeField] private TMP_Text requirementsLabel;
        [SerializeField] private Button executeButton;
        [SerializeField] private TMP_Text executeButtonLabel;
        [SerializeField] private Button surrenderButton;
        [SerializeField] private TMP_Text surrenderButtonLabel;

        [Header("Texts")]
        [SerializeField] private string peacefulTitle = "Всё спокойно";
        [SerializeField, TextArea] private string peacefulDescription = "День прошёл спокойно. Можно отправляться в следующую вылазку.";
        [SerializeField] private string continueButtonText = "Продолжить";
        [SerializeField] private string executeButtonText = "Выполнить";
        [SerializeField] private string surrenderButtonText = "Сдаться";
        [SerializeField] private string costTextFormat = "Стоимость: {0}$";
        [SerializeField] private string balanceTextFormat = "У вас: {0}$";

        [Header("Game Over")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TMP_Text gameOverLabel;
        [SerializeField, TextArea] private string gameOverText = "Семья от вас отвернулась, вы в экзистенциальной депрессии.";
        [SerializeField, Min(0)] private float restartDelay = 8f;

        [Inject] private PlayerBank _playerBank;
        [Inject] private PlayerState _playerState;

        private int _startedRaidsCount;
        private bool _isOpen;
        private bool _isResolving;
        private bool _isGameOver;
        private bool _wasSecondInteractionBlocked;
        private PlayerInteractStates _previousState;
        private DayEventSO _currentEvent;
        private Action _onDayEnded;

        public bool IsOpen => _isOpen;

        private void Start()
        {
            if (eventPanel)
                eventPanel.SetActive(false);

            if (gameOverPanel)
                gameOverPanel.SetActive(false);

            if (executeButton)
                executeButton.onClick.AddListener(OnExecuteClicked);

            if (surrenderButton)
                surrenderButton.onClick.AddListener(OnSurrenderClicked);

            _playerBank.OnMoneyChanged += OnMoneyChanged;
        }

        public void OpenDayEnd(Action onDayEnded)
        {
            if (_isOpen || !_playerState.IsInShelter())
                return;

            if (!eventPanel || !titleLabel || !descriptionLabel || !requirementsLabel ||
                !executeButton || !executeButtonLabel || !surrenderButton || !surrenderButtonLabel ||
                !gameOverPanel || !gameOverLabel)
            {
                Debug.LogError("[DayEndView] Assign event and game over UI references in the Inspector!");
                return;
            }

            _currentEvent = null;

            if (_startedRaidsCount >= peacefulRaidsCount)
            {
                var availableEvents = events.FindAll(item => item != null);

                if (availableEvents.Count == 0)
                {
                    Debug.LogError("[DayEndView] Assign at least one day event in the Inspector!");
                    return;
                }

                _currentEvent = availableEvents[Random.Range(0, availableEvents.Count)];
            }

            _onDayEnded = onDayEnded;
            _previousState = _playerState.GetCurrentState();
            _wasSecondInteractionBlocked = _playerState.IsSecondInteractionBlocked();
            _isOpen = true;
            _isResolving = false;
            _isGameOver = false;

            _playerState.ChangeBlockSecondInteraction(true);
            _playerState.SetCurrentInteractionState(PlayerInteractStates.LOCKED);
            gameOverPanel.SetActive(false);
            eventPanel.SetActive(true);
            Redraw();
        }

        private void Redraw()
        {
            var isPeaceful = _currentEvent == null;
            var canExecute = isPeaceful || _playerBank.Money >= _currentEvent.Cost;

            titleLabel.text = isPeaceful ? peacefulTitle : _currentEvent.Title;
            descriptionLabel.text = isPeaceful ? peacefulDescription : _currentEvent.Description;
            requirementsLabel.text = isPeaceful ? string.Empty :
                $"{_currentEvent.RequirementsDescription}\n{string.Format(costTextFormat, _currentEvent.Cost)}\n{string.Format(balanceTextFormat, _playerBank.Money)}";
            executeButtonLabel.text = isPeaceful ? continueButtonText : $"{executeButtonText} (-{_currentEvent.Cost}$)";
            surrenderButtonLabel.text = surrenderButtonText;

            executeButton.gameObject.SetActive(canExecute);
            surrenderButton.gameObject.SetActive(!canExecute);
            executeButton.interactable = !_isResolving;
            surrenderButton.interactable = !_isResolving;
        }

        private void OnExecuteClicked()
        {
            if (!_isOpen || _isResolving || _isGameOver)
                return;

            _isResolving = true;

            if (_currentEvent != null && !_playerBank.TryRemoveMoney(_currentEvent.Cost))
            {
                _isResolving = false;
                Redraw();
                return;
            }

            _startedRaidsCount++;
            _isOpen = false;
            eventPanel.SetActive(false);
            _playerState.ChangeBlockSecondInteraction(_wasSecondInteractionBlocked);
            _playerState.SetCurrentInteractionState(_previousState);

            var onDayEnded = _onDayEnded;
            _onDayEnded = null;
            _currentEvent = null;
            onDayEnded?.Invoke();
        }

        private void OnSurrenderClicked()
        {
            if (!_isOpen || _isResolving || _isGameOver || _currentEvent == null ||
                _playerBank.Money >= _currentEvent.Cost)
                return;

            _isGameOver = true;
            _onDayEnded = null;
            eventPanel.SetActive(false);
            gameOverLabel.text = gameOverText;
            gameOverPanel.SetActive(true);
            StartCoroutine(RestartAfterDelay());
        }

        private void OnMoneyChanged(int oldValue, int newValue)
        {
            if (_isOpen && !_isGameOver)
                Redraw();
        }

        private IEnumerator RestartAfterDelay()
        {
            yield return new WaitForSecondsRealtime(restartDelay);

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            if (executeButton)
                executeButton.onClick.RemoveListener(OnExecuteClicked);

            if (surrenderButton)
                surrenderButton.onClick.RemoveListener(OnSurrenderClicked);

            if (_playerBank != null)
                _playerBank.OnMoneyChanged -= OnMoneyChanged;
        }
    }
}
