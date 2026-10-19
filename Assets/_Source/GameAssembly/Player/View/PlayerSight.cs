using GameAssembly.Player.Data;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace GameAssembly.Player.View
{
    public class PlayerSight : MonoBehaviour
    {
        [SerializeField] private Image sight;

        [Inject] private PlayerState _playerState;

        private void Start()
        {
            _playerState.OnInteractionStateChanged += OnInteractionStateChanged;
        }

        private void OnDestroy()
        {
            _playerState.OnInteractionStateChanged -= OnInteractionStateChanged;
        }

        private void OnInteractionStateChanged(PlayerInteractStates state) => sight.enabled = state is PlayerInteractStates.CAMERA_ONLY or PlayerInteractStates.FREE;
    }
}