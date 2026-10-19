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

        public Transform GetShelterPoint() => shelterPoint;
        
        public void EndLootLoose()
        {
            _playerInventory.ClearInventory();
            _playerMovement.Teleport(shelterPoint.position);
            _playerState.SwitchShelterState(true);
        }
    }
}