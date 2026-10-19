using GameAssembly.Player;
using UnityEngine;
using VContainer;

namespace GameAssembly.LootLocation
{
    public class LocationExit : MonoBehaviour, IInteractable
    {
        [Inject] private LootPhaseEnd _lootPhaseEnd;
        [Inject] private PlayerState _playerState;
        [Inject] private PlayerMovement _playerMovement;
        [Inject] private PlayerCamera _playerCamera;

        public bool IsInteractable() => !_playerState.IsInShelter();

        public void Interact(object sender)
        {
            _playerState.SwitchShelterState(true);
            _playerMovement.Teleport(_lootPhaseEnd.GetShelterPoint().position);
            _playerCamera.SetRotation(_lootPhaseEnd.GetShelterPoint().rotation);
        }
    }
}