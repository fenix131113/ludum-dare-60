using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private List<Sprite> snakesSprites;
        [SerializeField] private Sprite ropeSprite;
        [SerializeField] private Rope[] ropes;
        [SerializeField] private int maxAttempts;

        [Inject] private PlayerState _playerState;

        private int _currentAttempts;
        private bool _isPlaying;

        public override event Action<bool> OnMinigameEnded;

        public override void StartMinigame()
        {
            _playerState.SetCurrentInteractionState(PlayerInteractStates.LOCKED);
            _isPlaying = true;
            ResetMinigame();
            minigamePanel.SetActive(true);
        }

        public override void ResetMinigame()
        {
            _currentAttempts = maxAttempts;
            var snakes = new List<Sprite>(snakesSprites);

            var valid = ropes[Random.Range(0, ropes.Length)];
            valid.Initialize(ropeSprite, true);

            foreach (var rope in ropes.Except(new[] { valid }))
            {
                var rnd = Random.Range(0, snakes.Count);

                rope.Initialize(snakes.Count > 0 ? snakes[rnd] : null);

                if (snakes.Count > 0)
                    snakes.RemoveAt(rnd);
            }
        }

        public override void WinMinigame()
        {
            _playerState.SetCurrentInteractionState(PlayerInteractStates.FREE);
            minigamePanel.SetActive(false);
            _isPlaying = false;
            OnMinigameEnded?.Invoke(true);
        }

        public override void LooseMinigame()
        {
            _playerState.SetCurrentInteractionState(PlayerInteractStates.FREE);
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

            if (rope.IsValid())
                WinMinigame();
            else
            {
                _currentAttempts -= 1;
                if (_currentAttempts <= 0)
                    LooseMinigame();
            }
        }
    }
}