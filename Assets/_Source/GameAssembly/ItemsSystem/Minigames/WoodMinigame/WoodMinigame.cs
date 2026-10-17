using System;
using GameAssembly.Player;
using GameAssembly.Player.Data;
using UnityEngine;
using VContainer;

namespace GameAssembly.ItemsSystem.Minigames.WoodMinigame
{
    public class WoodMinigame : AMinigame
    {
        [SerializeField] private GameObject minigamePanel;
        [SerializeField] private Nail[] nails;
        
        [Inject] private PlayerState _playerState;

        private int _nailCounter;
        private bool _isPlaying;

        public override event Action<bool> OnMinigameEnded;

        public override void StartMinigame()
        {
            _playerState.SetCurrentState(PlayerInteractStates.LOCKED);
            _isPlaying = true;
            minigamePanel.SetActive(true);
            ResetMinigame();
        }

        public override void ResetMinigame()
        {
            foreach (var nail in nails)
                nail.Initialize();

            _nailCounter = 0;
        }

        public override void WinMinigame()
        {
            _playerState.SetCurrentState(PlayerInteractStates.FREE);
            minigamePanel.SetActive(false);
            _isPlaying = false;
            OnMinigameEnded?.Invoke(true);
        }

        public override void LooseMinigame()
        {
            _playerState.SetCurrentState(PlayerInteractStates.FREE);
            minigamePanel.SetActive(false);
            _isPlaying = false;
            OnMinigameEnded?.Invoke(false);
        }

        public override bool IsPlaying() => _isPlaying;
        
        private void Start()
        {
            foreach (var nail in nails)
                nail.OnNailGet += OnNailGet;
        }

        private void OnDestroy()
        {
            foreach (var nail in nails)
                nail.OnNailGet -= OnNailGet;
        }

        private void OnNailGet(Nail nail)
        {
            _nailCounter++;

            if (_nailCounter >= nails.Length)
                WinMinigame();
        }
    }
}