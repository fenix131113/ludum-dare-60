using System.Collections.Generic;
using GameAssembly.Core;
using GameAssembly.Player;
using GameAssembly.Player.Data;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class CraftView : MonoBehaviour
    {
        [SerializeField] private GameObject craftsPanel;
        [SerializeField] private RectTransform craftsContent;
        [SerializeField] private CraftCell craftCellPrefab;
        [SerializeField] private List<CraftItemSO> crafts;
        [SerializeField] private List<CraftItemSO> blockedCrafts;

        [Inject] private PlayerState _playerState;
        [Inject] private PlayerCamera _playerCamera;
        [Inject] private PlayerInput _input;

        private bool _isCrafting;
        private CraftItemSO _currentCraft;

        private readonly List<CraftCell> _craftCells = new();
        
        public IReadOnlyCollection<CraftItemSO> Crafts => crafts;
        
        public void UnblockCraft(CraftItemSO craft)
        {
            if(!blockedCrafts.Contains(craft))
                return;
            
            blockedCrafts.Remove(craft);
        }

        public void StartCrafting(CraftItemSO craft)
        {
            craftsPanel.SetActive(false);
            _isCrafting = true;
            _currentCraft = craft;
        }
        
        public void OpenCraftMenu()
        {
            craftsPanel.SetActive(true);
            _input.OnInteractPerformed += OnInteractionClicked;
            _playerState.SetCurrentInteractionState(PlayerInteractStates.LOCKED);
            Redraw();
        }

        public void CloseCraftMenu()
        {
            craftsPanel.SetActive(false);
            _input.OnInteractPerformed -= OnInteractionClicked;
            _playerState.SetCurrentInteractionState(PlayerInteractStates.FREE);
            _playerCamera.ResetCameraToStartPosition();
        }
        
        public void Redraw()
        {
            _craftCells.ForEach(x => Destroy(x.gameObject));
            _craftCells.Clear();
            
            foreach (var craft in crafts)
            {
                if(blockedCrafts.Contains(craft))
                    continue;
                
                var spawned = Instantiate(craftCellPrefab,  craftsContent);
                spawned.Initialize(craft, this);
                GameInstaller.InjectGameObject(spawned.gameObject);
                _craftCells.Add(spawned);
            }
        }

        private void OnInteractionClicked()
        {
            if(_isCrafting)
                return;
            
            CloseCraftMenu();
        }
    }
}