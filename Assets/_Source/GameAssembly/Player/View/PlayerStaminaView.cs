using GameAssembly.Player.Data;
using UnityEngine;
using VContainer;

namespace GameAssembly.Player.View
{
    public class PlayerStaminaView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup staminaGroup;
        [SerializeField] private RectTransform fill;
        [SerializeField, Min(0f)] private float hideDelay = 1f;

        [Inject] private PlayerStamina _playerStamina;
        [Inject] private PlayerState _playerState;

        private float _fullTime;
        private bool _wasUsed;

        private void Awake()
        {
            staminaGroup.alpha = 0f;
            staminaGroup.interactable = false;
            staminaGroup.blocksRaycasts = false;
        }

        private void Update()
        {
            var value = _playerStamina.Value;
            fill.anchorMin = new Vector2(0.5f - value * 0.5f, 0f);
            fill.anchorMax = new Vector2(0.5f + value * 0.5f, 1f);

            if (value < 1f)
            {
                _wasUsed = true;
                _fullTime = 0f;
            }
            else
                _fullTime += Time.deltaTime;

            var state = _playerState.GetCurrentState();
            var canMove = state == PlayerInteractStates.FREE || state == PlayerInteractStates.MOVEMENT_ONLY;
            staminaGroup.alpha = _wasUsed && _fullTime < hideDelay && canMove &&
                                 !_playerState.IsInTwoD() && !_playerState.IsSecondInteractionBlocked() ? 1f : 0f;
        }
    }
}
