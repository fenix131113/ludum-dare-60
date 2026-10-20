using GameAssembly.ItemsSystem;
using GameAssembly.Player;
using TMPro;
using UnityEngine;
using VContainer;

namespace GameAssembly.TraderSystem.View.ShopCells
{
    public class TakeStoryQuestCell : BaseShopCell
    {
        [SerializeField] private string takeButtonText = "Take quest";
        [SerializeField] private TMP_Text descriptionLabel;

        [Inject] private StoryQuestSystem _storyQuestSystem;
        [Inject] private PlayerBank _playerBank;

        private BlueprintItemDataSO _quest;

        public void Initialize(BlueprintItemDataSO quest)
        {
            _quest = quest;
            base.Initialize(quest.Craft.ItemData, 1, takeButtonText);
            costLabel.text = quest.Cost == 0 ? "" : quest.Cost + "$";

            if (descriptionLabel)
                descriptionLabel.text = quest.Description;
        }

        public override bool CanUse() => _storyQuestSystem.CanTakeQuest(_quest);

        public override void Use() => _storyQuestSystem.TryTakeQuest(_quest);

        protected override void Start()
        {
            base.Start();
            _playerBank.OnMoneyChanged += OnMoneyChanged;
            _storyQuestSystem.OnQuestChanged += Redraw;
            Redraw();
        }

        private void OnMoneyChanged(int oldValue, int newValue) => Redraw();

        private void Redraw() => useButton.interactable = CanUse();

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_playerBank != null)
                _playerBank.OnMoneyChanged -= OnMoneyChanged;

            if (_storyQuestSystem != null)
                _storyQuestSystem.OnQuestChanged -= Redraw;
        }
    }
}
