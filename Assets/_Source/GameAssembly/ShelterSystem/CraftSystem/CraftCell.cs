using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class CraftCell : MonoBehaviour
    {
        [SerializeField] private RectTransform craftRequirementPrefab;
        [SerializeField] private RectTransform requirementsContent;
        [SerializeField] private Button craftButton;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text nameLabel;

        [Inject] private ShelterInventory _inventory;

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
        }

        private void OnDestroy()
        {
            craftButton.onClick.RemoveListener(OnCraftButtonClicked);
        }

        private void OnCraftButtonClicked()
        {
            var requirementsComplete =
                _craft.CraftsRequirements.All(req => _inventory.IsItemInInventory(req.ItemData, req.Count));
            
            if(!requirementsComplete)
                return;

            _view.StartCrafting(_craft);
        }
    }
}