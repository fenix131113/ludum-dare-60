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
        [SerializeField] private CraftInventoryCell inventoryCellPrefab;
        [SerializeField] private LockedBlueprintCell lockedBlueprintCellPrefab;
        [SerializeField] private RectTransform craftsContent;
        [SerializeField] private RectTransform inventoryContent;
        [SerializeField] private RectTransform lockedBlueprintsContent;
        [SerializeField] private CraftCell craftCellPrefab;
        [SerializeField] private List<CraftItemSO> crafts;
        [SerializeField] private List<CraftItemSO> blockedCrafts;

        [Header("Physical Crafting")]
        [SerializeField] private CraftPhysicalGame physicalGame;

        [Inject] private PlayerState _playerState;
        [Inject] private PlayerCamera _playerCamera;
        [Inject] private PlayerInput _input;
        [Inject] private CraftPhysicalGame _craftPhysicalGame;
        [Inject] private CraftItemTooltip _tooltip;
        [Inject] private ShelterInventory _inventory;

        private bool _isCrafting;
        private CraftItemSO _currentCraft;

        private readonly List<CraftCell> _craftCells = new();
        
        public IReadOnlyCollection<CraftItemSO> Crafts => crafts;

        public bool IsCraftBlocked(CraftItemSO craft) => blockedCrafts.Contains(craft);

        public void BlockCraft(CraftItemSO craft)
        {
            if (!blockedCrafts.Contains(craft))
                blockedCrafts.Add(craft);
        }
        
        public void UnblockCraft(CraftItemSO craft)
        {
            if(!blockedCrafts.Contains(craft))
                return;
            
            blockedCrafts.Remove(craft);
        }

        public void StartCrafting(CraftItemSO craft)
        {
            _tooltip.Hide();
            craftsPanel.SetActive(false);
            _isCrafting = true;
            _currentCraft = craft;

            if (_craftPhysicalGame == null)
                _craftPhysicalGame = physicalGame != null ? physicalGame : FindAnyObjectByType<CraftPhysicalGame>();

            if (_craftPhysicalGame != null)
            {
                _craftPhysicalGame.StartCraft(craft, OnCraftFinished, OnCraftCancelled);
            }
            else
            {
                Debug.LogWarning("[CraftView] CraftPhysicalGame not found in scene!");
            }
        }

        private void OnCraftFinished()
        {
            _isCrafting = false;
            _currentCraft = null;
            CloseCraftMenu();
        }

        private void OnCraftCancelled()
        {
            _isCrafting = false;
            _currentCraft = null;
            craftsPanel.SetActive(true);
            Redraw();
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
            _tooltip.Hide();
            craftsPanel.SetActive(false);
            _input.OnInteractPerformed -= OnInteractionClicked;
            _playerState.SetCurrentInteractionState(PlayerInteractStates.FREE);
            _playerCamera.ResetCameraToStartPosition();
        }
        
        public void Redraw()
        {
            _tooltip.Hide();
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
            
            RedrawInventory();
            RedrawLockedBlueprints();
        }

        private void RedrawInventory()
        {
            for (var i = 0; i < inventoryContent.childCount; i++)
                Destroy(inventoryContent.GetChild(i).gameObject);
            
            foreach (var item in _inventory.Items)
                Instantiate(inventoryCellPrefab, inventoryContent).Initialize(item.Key, item.Value);
        }

        private void RedrawLockedBlueprints()
        {
            for (var i = 0; i < lockedBlueprintsContent.childCount; i++)
                Destroy(lockedBlueprintsContent.GetChild(i).gameObject);
            
            foreach (var craft in blockedCrafts)
                Instantiate(lockedBlueprintCellPrefab, lockedBlueprintsContent).Initialize(craft.ItemData);
        }

        private void OnInteractionClicked()
        {
            if(_isCrafting)
            {
                if (_craftPhysicalGame != null)
                    _craftPhysicalGame.CancelCraft();
                return;
            }
            
            CloseCraftMenu();
        }
    }
}
