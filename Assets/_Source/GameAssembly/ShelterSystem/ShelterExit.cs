using GameAssembly.LootLocation.Generation;
using GameAssembly.Player;
using UnityEngine;
using VContainer;

namespace GameAssembly.ShelterSystem
{
    public class ShelterExit : MonoBehaviour, IInteractable
    {
        [Inject] private PlayerState _playerState;
        [Inject] private PlayerMovement _playerMovement;
        [Inject] private LocationGenerator _locationGenerator;
        [Inject] private PlayerCamera _playerCamera;
        
        public bool IsInteractable() => _playerState.IsInShelter();

        public void Interact(object sender)
        {
            _playerState.SwitchShelterState(false);
            
            _locationGenerator.GenerateLocation();
            
            _playerMovement.Teleport(_locationGenerator.GetEnterPoint().position);
            _playerCamera.SetRotation(_locationGenerator.GetEnterPoint().rotation);
        }
    }
}