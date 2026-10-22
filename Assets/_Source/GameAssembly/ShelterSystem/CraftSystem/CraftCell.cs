using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class CraftCell : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        [SerializeField] private RectTransform craftRequirementPrefab;
        [SerializeField] private RectTransform requirementsContent;
        [SerializeField] private GameObject lockObject;
        [SerializeField] private Button craftButton;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameLabel;

        [Inject] private ShelterInventory _inventory;
        [Inject] private CraftItemTooltip _tooltip;

        private CraftItemSO _craft;
        private CraftView _view;

        public void Initialize(CraftItemSO craft, CraftView view)
        {
            _craft = craft;
            _view = view;
            
            icon.sprite = craft.ItemData.Icon;
            nameLabel.text = craft.ItemData.ItemName;
            
            foreach (var req in _craft.CraftsRequirements)
            {
                var spawned = Instantiate(craftRequirementPrefab, requirementsContent);
                spawned.GetChild(0).GetComponent<Image>().sprite = req.ItemData.Icon;
                spawned.GetChild(1).GetComponent<TMP_Text>().text = "x" + req.Count.ToString();
            }
        }

        private void Start()
        {
            craftButton.onClick.AddListener(OnCraftButtonClicked);
            var canCraft = CanCraft();
            craftButton.interactable = false;
            lockObject.SetActive(!canCraft);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_craft)
                _tooltip.Show(this, _craft.ItemData, eventData.position, eventData.enterEventCamera);
        }

        public void OnPointerMove(PointerEventData eventData) =>
            _tooltip.Move(this, eventData.position, eventData.enterEventCamera);

        public void OnPointerExit(PointerEventData eventData) => _tooltip.Hide(this);

        private void OnDisable()
        {
            if (_tooltip != null)
                _tooltip.Hide(this);
        }

        private void OnDestroy()
        {
            craftButton.onClick.RemoveListener(OnCraftButtonClicked);
        }

        private bool CanCraft() =>
            _craft.CraftsRequirements.All(req => _inventory.IsItemInInventory(req.ItemData, req.Count));
        
        private void OnCraftButtonClicked()
        {
            var requirementsComplete = CanCraft();
            
            if(!requirementsComplete)
                return;
            
            _view.StartCrafting(_craft);
        }
    }
}
