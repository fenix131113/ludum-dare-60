using System;
using System.Collections.Generic;
using System.Linq;
using GameAssembly.ItemsSystem;
using GameAssembly.Player;
using GameAssembly.ShelterSystem;
using GameAssembly.ShelterSystem.CraftSystem;
using VContainer;

namespace GameAssembly.TraderSystem
{
    public class StoryQuestSystem
    {
        [Inject] private CraftView _craftView;
        [Inject] private PlayerBank _playerBank;
        [Inject] private ShelterInventory _shelterInventory;

        private readonly HashSet<BlueprintItemDataSO> _quests = new();
        private readonly HashSet<CraftItemSO> _completedCrafts = new();
        private bool _isChangingQuest;

        public event Action OnQuestChanged;

        public BlueprintItemDataSO ActiveQuest { get; private set; }

        public void RegisterQuests(IEnumerable<BlueprintItemDataSO> quests)
        {
            foreach (var quest in quests)
            {
                if (!quest || !quest.Craft || !quest.Craft.ItemData || !quest.Craft.ItemData.StoryItem ||
                    !_craftView.Crafts.Contains(quest.Craft) || !_quests.Add(quest))
                    continue;

                if (!ActiveQuest || ActiveQuest.Craft != quest.Craft)
                    _craftView.BlockCraft(quest.Craft);
            }
        }

        public bool IsQuestAvailable(BlueprintItemDataSO quest) => quest && _quests.Contains(quest) &&
            !_completedCrafts.Contains(quest.Craft);

        public bool CanTakeQuest(BlueprintItemDataSO quest) => !_isChangingQuest && !ActiveQuest &&
            IsQuestAvailable(quest) && quest.Cost >= 0 && _playerBank.Money >= quest.Cost;

        public bool TryTakeQuest(BlueprintItemDataSO quest)
        {
            if (!CanTakeQuest(quest))
                return false;

            _isChangingQuest = true;

            if (!_playerBank.TryRemoveMoney(quest.Cost))
            {
                _isChangingQuest = false;
                return false;
            }

            ActiveQuest = quest;
            _craftView.UnblockCraft(quest.Craft);
            _isChangingQuest = false;
            OnQuestChanged?.Invoke();
            return true;
        }

        public bool CanSubmitQuest() => !_isChangingQuest && ActiveQuest &&
            _shelterInventory.IsItemInInventory(ActiveQuest.Craft.ItemData);

        public bool TrySubmitQuest()
        {
            if (!CanSubmitQuest())
                return false;

            _isChangingQuest = true;
            var quest = ActiveQuest;

            if (!_shelterInventory.TryRemoveItem(quest.Craft.ItemData))
            {
                _isChangingQuest = false;
                return false;
            }

            _completedCrafts.Add(quest.Craft);
            _craftView.BlockCraft(quest.Craft);
            ActiveQuest = null;
            _isChangingQuest = false;
            OnQuestChanged?.Invoke();
            return true;
        }
    }
}
