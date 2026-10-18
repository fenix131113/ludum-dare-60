using System;
using GameAssembly.Player;
using GameAssembly.Player.Data;
using UnityEngine;
using VContainer;

namespace GameAssembly.ItemsSystem.Minigames.PaperMinigame
{
    public class PaperMinigame : AMinigame
    {
        [SerializeField] private GameObject minigamePanel;
        [SerializeField] private FleeingPaper paper;

        [Inject] private PlayerState _playerState;

        private bool _isPlaying;

        public override event Action<bool> OnMinigameEnded;

        public override void StartMinigame()
        {
            _playerState.SetCurrentInteractionState(PlayerInteractStates.LOCKED);
            _isPlaying = true;
            minigamePanel.SetActive(true);
            ResetMinigame();
        }

        public override void ResetMinigame()
        {
            paper.transform.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            paper.ResetPaperState();
            paper.gameObject.SetActive(true);
        }

        public override void WinMinigame()
        {
            if (!_isPlaying)
                return;

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
            paper.OnPaperCaught += WinMinigame;
        }

        private void OnDestroy()
        {
            paper.OnPaperCaught -= WinMinigame;
        }
    }
}