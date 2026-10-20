using GameAssembly.Core;
using GameAssembly.Player;
using UnityEngine;
using VContainer;

namespace GameAssembly.LootLocation
{
    public class LootPhaseEnd : MonoBehaviour
    {
        [SerializeField] private Transform shelterPoint;
        
        [Inject] private PlayerState _playerState;
        [Inject] private PlayerMovement _playerMovement;
        [Inject] private PlayerInventory _playerInventory;
        [Inject] private PlayerCamera _playerCamera;
        [Inject] private ScreenTransition _screenTransition;

        public Transform GetShelterPoint() => shelterPoint;
        
        public void EndLootLoose()
        {
            _screenTransition.Transition(() =>
            {
                _playerInventory.ClearInventory();
                _playerMovement.Teleport(shelterPoint.position);
                _playerCamera.SetRotation(shelterPoint.rotation);
                _playerState.SwitchShelterState(true);
            });
        }
    }
}