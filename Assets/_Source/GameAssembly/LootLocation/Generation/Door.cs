using GameAssembly.Core;
using GameAssembly.Player;
using UnityEngine;
using VContainer;

namespace GameAssembly.LootLocation.Generation
{
    public class Door : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform spawnPoint;
            
        [Inject] private PlayerMovement _playerMovement;
        [Inject] private PlayerCamera _playerCamera;
        [Inject] private ScreenTransition _screenTransition;

        private Transform _pointToTeleport;
        
        public void InitDoor(Transform pointToTeleport)
        {
            _pointToTeleport = pointToTeleport;
        }

        public Transform GetSpawnPoint() => spawnPoint;
        public Transform GetPointToTeleport() => _pointToTeleport;

        public bool IsInteractable() => _pointToTeleport && !_screenTransition.IsTransitioning;

        public void Interact(object sender)
        {
            if (!IsInteractable())
                return;

            _screenTransition.Transition(() =>
            {
                var parent = _pointToTeleport.parent;
                var worldPosition = parent ? parent.TransformPoint(_pointToTeleport.localPosition) : _pointToTeleport.localPosition;
                var worldRotation = parent ? parent.rotation * _pointToTeleport.localRotation : _pointToTeleport.localRotation;

                _playerMovement.Teleport(worldPosition);
                _playerCamera.SetRotation(worldRotation);
            });
        }
    }
}
