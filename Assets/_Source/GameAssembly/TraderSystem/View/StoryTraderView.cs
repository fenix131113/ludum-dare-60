using System.Collections.Generic;
using GameAssembly.ItemsSystem;
using GameAssembly.TraderSystem.View.ShopCells;
using UnityEngine;
using VContainer;

namespace GameAssembly.TraderSystem.View
{
    public class StoryTraderView : TraderView
    {
        [SerializeField] private List<BlueprintItemDataSO> quests = new();
        [SerializeField] private TakeStoryQuestCell takeQuestCellPrefab;
        [SerializeField] private SubmitStoryQuestCell submitQuestCellPrefab;
        [SerializeField] private Transform questsContent;

        [Inject] private StoryQuestSystem _storyQuestSystem;

        private readonly List<BaseShopCell> _questCells = new();
        private bool _isOpen;
        private bool _isInitialized;

        private void Start() => Initialize();

        private void Initialize()
        {
            if (_isInitialized)
                return;

            _storyQuestSystem.RegisterQuests(quests);
            _storyQuestSystem.OnQuestChanged += OnQuestChanged;
            _isInitialized = true;
        }

        public override void OpenTraderView()
        {
            if (_isOpen)
                return;

            Initialize();
            base.OpenTraderView();
            _isOpen = true;
            Redraw();
        }

        public override void CloseTraderView()
        {
            _isOpen = false;
            base.CloseTraderView();
            ClearCells();
        }

        private void OnQuestChanged()
        {
            if (_isOpen)
                Redraw();
        }

        private void Redraw()
        {
            ClearCells();

            if (_storyQuestSystem.ActiveQuest)
            {
                var spawned = Instantiate(submitQuestCellPrefab, questsContent);
                spawned.Initialize(_storyQuestSystem.ActiveQuest);
                _questCells.Add(spawned);
                return;
            }

            var addedQuests = new HashSet<BlueprintItemDataSO>();

            foreach (var quest in quests)
            {
                if (!_storyQuestSystem.IsQuestAvailable(quest) || !addedQuests.Add(quest))
                    continue;

                var spawned = Instantiate(takeQuestCellPrefab, questsContent);
                spawned.Initialize(quest);
                _questCells.Add(spawned);
            }
        }

        private void ClearCells()
        {
            foreach (var cell in _questCells)
            {
                if (!cell)
                    continue;

                cell.gameObject.SetActive(false);
                Destroy(cell.gameObject);
            }

            _questCells.Clear();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_storyQuestSystem != null)
                _storyQuestSystem.OnQuestChanged -= OnQuestChanged;
        }
    }
}
