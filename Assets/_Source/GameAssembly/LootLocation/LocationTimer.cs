using System.Collections;
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

        private float _currentTimer;

        private void Start()
        {
            _playerState.OnShelterStateChanged += OnShelterStateChanged;
        }

        private void Update()
        {
            if (_playerState.IsInShelter() || _currentTimer < 0)
                return;

            _currentTimer -= Time.deltaTime;

            if (_currentTimer <= 0)
            {
                StartCoroutine(LooseDelay());
            }

            if(_currentTimer < 0)
                return;
            
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
            yield return new WaitForSeconds(endDelay);

            Loose();
        }
    }
}