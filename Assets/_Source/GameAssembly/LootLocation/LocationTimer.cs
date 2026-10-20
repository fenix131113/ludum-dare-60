using System.Collections;
using GameAssembly.Core;
using GameAssembly.Player;
using TMPro;
using UnityEngine;
using VContainer;

namespace GameAssembly.LootLocation
{
    public class LocationTimer : MonoBehaviour
    {
        [SerializeField] private float time;
        [SerializeField] private float endDelay = 10f;
        [SerializeField] private TMP_Text timeLabel;
        [SerializeField] private GameObject clockObject;

        [Inject] private LootPhaseEnd _lootPhaseEnd;
        [Inject] private PlayerState _playerState;
        [Inject] private ScreenTransition _screenTransition;

        private float _currentTimer;
        private bool _isEnding;

        private void Start()
        {
            _playerState.OnShelterStateChanged += OnShelterStateChanged;
        }

        private void Update()
        {
            if (_playerState.IsInShelter() || _isEnding || _screenTransition.IsTransitioning)
                return;

            _currentTimer = Mathf.Max(0f, _currentTimer - Time.deltaTime);

            if (_currentTimer <= 0)
            {
                _isEnding = true;
                StartCoroutine(LooseDelay());
            }

            timeLabel.text = Mathf.FloorToInt(_currentTimer / 60).ToString("00") + ":" +
                             Mathf.FloorToInt(_currentTimer % 60).ToString("00");
        }

        private void OnDestroy()
        {
            _playerState.OnShelterStateChanged -= OnShelterStateChanged;
        }

        private void Loose()
        {
            _lootPhaseEnd.EndLootLoose();
        }

        private void OnShelterStateChanged()
        {
            StopAllCoroutines();
            _isEnding = false;

            if (_playerState.IsInShelter())
            {
                clockObject.SetActive(false);
                return;
            }

            clockObject.SetActive(true);
            _currentTimer = time;
        }

        private IEnumerator LooseDelay()
        {
            var remaining = endDelay;

            while (remaining > 0f)
            {
                yield return null;

                if (!_screenTransition.IsTransitioning)
                    remaining -= Time.deltaTime;
            }

            while (_screenTransition.IsTransitioning)
                yield return null;

            Loose();
        }
    }
}
