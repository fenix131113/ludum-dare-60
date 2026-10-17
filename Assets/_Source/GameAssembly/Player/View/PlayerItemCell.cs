using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameAssembly.Player.View
{
    public class PlayerItemCell : MonoBehaviour
    {
        [field: SerializeField] public Image Icon { get; private set; }
        [field: SerializeField] public TMP_Text Counter { get; private set; }
        [field: SerializeField] public TMP_Text ItemNameLabel { get; private set; }

        public void Initialize(Sprite icon, int count, string itemName)
        {
            Icon.sprite = icon;
            Counter.text = "x" + count.ToString();
            ItemNameLabel.text = itemName;
        }
    }
}