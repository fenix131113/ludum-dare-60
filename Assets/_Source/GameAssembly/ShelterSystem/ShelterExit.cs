using GameAssembly.Core;
using GameAssembly.LootLocation.Generation;
using GameAssembly.Player;
using GameAssembly.Player.Data;
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
        [Inject] private DayEndView _dayEndView;
        [Inject] private ScreenTransition _screenTransition;
        
        public bool IsInteractable() => _playerState.IsInShelter() && !_dayEndView.IsOpen && !_screenTransition.IsTransitioning &&
                                        _playerState.GetCurrentState() == PlayerInteractStates.FREE;

        public void Interact(object sender)
        {
            if (!IsInteractable())
                return;

            _dayEndView.OpenDayEnd(StartRaid);
        }

        private void StartRaid()
        {
            _screenTransition.Transition(() =>
            {
                _locationGenerator.GenerateLocation();
                _playerMovement.Teleport(_locationGenerator.GetEnterPoint().position);
                _playerCamera.SetRotation(_locationGenerator.GetEnterPoint().rotation);
                _playerState.SwitchShelterState(false);
            });
        }
    }
}
