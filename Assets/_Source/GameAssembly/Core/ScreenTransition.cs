using System;
using System.Collections;
using GameAssembly.Player;
using UnityEngine;
using VContainer;

namespace GameAssembly.Core
{
    public class ScreenTransition : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeGroup;
        [SerializeField, Min(0f)] private float fadeDuration = 0.5f;

        [Inject] private PlayerState _playerState;

        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (fadeGroup)
            {
                fadeGroup.alpha = 0f;
                fadeGroup.blocksRaycasts = false;
            }
        }

        public void Transition(Action onScreenBlack)
        {
            if (IsTransitioning || onScreenBlack == null)
                return;

            if (!fadeGroup)
            {
                Debug.LogError("[ScreenTransition] Assign the fade CanvasGroup in the Inspector!");
                return;
            }

            IsTransitioning = true;
            _playerState.LockControls(this);
            fadeGroup.transform.SetAsLastSibling();
            fadeGroup.blocksRaycasts = true;
            StartCoroutine(PlayTransition(onScreenBlack));
        }

        private IEnumerator PlayTransition(Action onScreenBlack)
        {
            try
            {
                yield return Fade(0f, 1f);
                onScreenBlack();
                yield return Fade(1f, 0f);
            }
            finally
            {
                FinishTransition();
            }
        }

        private IEnumerator Fade(float from, float to)
        {
            var elapsed = 0f;
            fadeGroup.alpha = from;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeGroup.alpha = Mathf.Lerp(from, to, fadeDuration > 0f ? elapsed / fadeDuration : 1f);
                yield return null;
            }

            fadeGroup.alpha = to;
        }

        private void FinishTransition()
        {
            if (!IsTransitioning)
                return;

            if (fadeGroup)
            {
                fadeGroup.alpha = 0f;
                fadeGroup.blocksRaycasts = false;
            }

            IsTransitioning = false;
            _playerState.UnlockControls(this);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            FinishTransition();
        }
    }
}
