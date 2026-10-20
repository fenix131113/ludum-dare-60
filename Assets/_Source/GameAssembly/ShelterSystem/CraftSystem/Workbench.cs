using GameAssembly.Player;
using UnityEngine;
using VContainer;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class Workbench : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform cameraPoint;
        [Inject] private PlayerCamera _playerCamera;
        [Inject] private CraftView _craftView;
        
        public bool IsInteractable() => true;

        public void Interact(object sender)
        {
            _craftView.OpenCraftMenu();
            _playerCamera.transform.position = cameraPoint.position;
            _playerCamera.transform.rotation = cameraPoint.rotation;
        }
    }
}