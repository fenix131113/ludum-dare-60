using System;
using GameAssembly.Player;
using GameAssembly.Player.Data;
using UnityEngine;
using VContainer;
using Random = UnityEngine.Random;

namespace GameAssembly.ItemsSystem.Minigames.RopeMinigame
{
    public class RopeMinigame : AMinigame
    {
        [SerializeField] private GameObject minigamePanel;
        [SerializeField] private Rope[] ropes;
        [SerializeField] private int maxAttempts;
        
        [Inject] private PlayerState _playerState;

        private int _currentAttempts;
        private bool _isPlaying;

        public override event Action<bool> OnMinigameEnded;

        public override void StartMinigame()
        {
            _playerState.SetCurrentState(PlayerInteractStates.LOCKED);
            _isPlaying = true;
            ResetMinigame();
            minigamePanel.SetActive(true);
        }

        public override void ResetMinigame()
        {
            _currentAttempts = maxAttempts;
            
            foreach (var rope in ropes)
                rope.Initialize();
            
            ropes[Random.Range(0, ropes.Length)].Initialize(true);
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
            foreach (var rope in ropes)
                rope.OnClicked += OnClickedRope;
        }

        private void OnDestroy()
        {
            foreach (var rope in ropes)
                rope.OnClicked -= OnClickedRope;
        }

        private void OnClickedRope(Rope rope)
        {
            rope.gameObject.SetActive(false);
            
            if(rope.IsValid())
                WinMinigame();
            else
            {
                _currentAttempts -= 1;
                if(_currentAttempts <= 0)
                    LooseMinigame();
            }
        }
    }
}