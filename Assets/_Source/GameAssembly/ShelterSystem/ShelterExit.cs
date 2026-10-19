using GameAssembly.Player;
using UnityEngine;
using VContainer;

namespace GameAssembly.ShelterSystem
{
    public class ShelterExit : MonoBehaviour, IInteractable
    {
        [Inject] private PlayerState _playerState;
        
        public bool IsInteractable() => true;

        public void Interact(object sender)
        {
            _playerState.SwitchShelterState(!_playerState.IsInShelter());
        }
    }
}