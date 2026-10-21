using UnityEngine;
using VContainer;

namespace GameAssembly.Player._2D
{
    public class Zone2D : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform cameraPoint;
        [SerializeField] private TradingZoneScroll zoneScroll;
        
        [Inject] private PlayerState _playerState;
        [Inject] private PlayerCamera _playerCamera;
        [Inject] private Player2DMovement _player2DMovement;

        public bool IsInteractable() => !_playerState.IsInTwoD();

        public void Interact(object sender)
        {
            _player2DMovement.SetScrollZone(zoneScroll);
            _playerState.SwitchDimension(true);
            _playerCamera.transform.position = cameraPoint.position;
            _playerCamera.transform.rotation = cameraPoint.rotation;
            _player2DMovement.Teleport(spawnPoint.position);
            _player2DMovement.transform.rotation = spawnPoint.rotation;
        }
    }
}
