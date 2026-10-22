using GameAssembly.ItemsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class LockedBlueprintCell : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemNameLabel;
        
        public void Initialize(ItemDataSO craftItem)
        {
            itemIcon.sprite = craftItem.Icon;
            itemNameLabel.text = craftItem.ItemName;
        }
    }
}