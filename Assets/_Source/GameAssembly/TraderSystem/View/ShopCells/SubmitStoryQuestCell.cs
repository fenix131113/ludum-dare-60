using GameAssembly.ItemsSystem;
using GameAssembly.ShelterSystem;
using UnityEngine;
using VContainer;

namespace GameAssembly.TraderSystem.View.ShopCells
{
    public class SubmitStoryQuestCell : BaseShopCell
    {
        [SerializeField] private string submitButtonText = "Give item";

        [Inject] private StoryQuestSystem _storyQuestSystem;
        [Inject] private ShelterInventory _shelterInventory;

        private BlueprintItemDataSO _quest;

        public void Initialize(BlueprintItemDataSO quest)
        {
            _quest = quest;
            base.Initialize(quest.Craft.ItemData, 1, submitButtonText);
            costLabel.text = "";
        }

        public override bool CanUse() => _storyQuestSystem.ActiveQuest == _quest && _storyQuestSystem.CanSubmitQuest();

        public override void Use()
        {
            if (CanUse())
                _storyQuestSystem.TrySubmitQuest();
        }

        protected override void Start()
        {
            base.Start();
            _storyQuestSystem.OnQuestChanged += Redraw;
            _shelterInventory.OnItemAdded += OnInventoryChanged;
            _shelterInventory.OnItemRemoved += OnInventoryChanged;
            Redraw();
        }

        private void OnInventoryChanged(ItemDataSO item, int count) => Redraw();

        private void Redraw()
        {
            useButton.interactable = CanUse();
            countLabel.text = _shelterInventory.IsItemInInventory(_quest.Craft.ItemData) ? "1/1" : "0/1";
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_storyQuestSystem != null)
                _storyQuestSystem.OnQuestChanged -= Redraw;

            if (_shelterInventory != null)
            {
                _shelterInventory.OnItemAdded -= OnInventoryChanged;
                _shelterInventory.OnItemRemoved -= OnInventoryChanged;
            }
        }
    }
}
