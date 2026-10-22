using GameAssembly.ItemsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class CraftInventoryCell : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemNameLabel;
        [SerializeField] private TMP_Text countLabel;

        private ItemDataSO _currentItem;

        public void Initialize(ItemDataSO item, int count)
        {
            itemIcon.sprite = item.Icon;
            itemNameLabel.text = item.ItemName;
            countLabel.text = "x" + count.ToString();
        }
    }
}