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

        private Transform _pointToTeleport;
        
        public void InitDoor(Transform pointToTeleport)
        {
            _pointToTeleport = pointToTeleport;
        }

        public Transform GetSpawnPoint() => spawnPoint;
        public Transform GetPointToTeleport() => _pointToTeleport;

        public bool IsInteractable() => true;

        public void Interact(object sender)
        {
            _playerMovement.Teleport(_pointToTeleport.position);
            _playerCamera.SetRotation(_pointToTeleport.rotation);
        }
    }
}