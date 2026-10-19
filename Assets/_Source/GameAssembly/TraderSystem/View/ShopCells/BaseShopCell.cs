using GameAssembly.Core;
using TMPro;
using GameAssembly.ItemsSystem;
using UnityEngine;
using UnityEngine.UI;

namespace GameAssembly.TraderSystem.View.ShopCells
{
    public class BaseShopCell : AShopCell
    {
        [SerializeField] protected Image iconImage;
        [SerializeField] protected TMP_Text costLabel;
        [SerializeField] protected TMP_Text nameLabel;
        [SerializeField] protected TMP_Text countLabel;
        [SerializeField] protected Button useButton;
        [SerializeField] protected TMP_Text useButtonLabel;

        public override void Initialize(ItemDataSO item, int count, string buttonLabel)
        {
            iconImage.sprite = item.Icon;
            nameLabel.text = item.ItemName;
            countLabel.text = "x" + count.ToString();
            costLabel.text = item.Cost == 0 ? "" : item.Cost.ToString() + "$";
            useButtonLabel.text = buttonLabel;
        }

        public override bool CanUse() => true;

        public override void Use()
        {
        }

        protected virtual void Start()
        {
            GameInstaller.InjectGameObject(gameObject);
            useButton.onClick.AddListener(OnButtonClicked);
        }

        protected virtual void OnDestroy()
        {
            useButton.onClick.RemoveAllListeners();
        }

        protected void OnButtonClicked()
        {
            if (CanUse())
                Use();
        }
    }
}
