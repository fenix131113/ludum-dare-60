using GameAssembly.Core;
using GameAssembly.ItemsSystem.Minigames;
using GameAssembly.LootLocation.Generation;
using GameAssembly.Player;
using GameAssembly.ShelterSystem;
using UnityEngine;
using VContainer;

namespace GameAssembly.ItemsSystem
{
    public class TakeableItem : MonoBehaviour, IInteractable
    {
        [field: SerializeField] public ItemDataSO ItemData { get; private set; }
        [SerializeField] private GameObject rootObject;
        [SerializeField] private bool minigame;
        [SerializeField] private int minigameIndex;
        [SerializeField] private ItemDataSO itemToSkipMinigame;

        [Inject] private PlayerInventory _inventory;
        [Inject] private BlueprintLootSystem _blueprintLootSystem;
        [Inject] private ShelterInventory _shelterInventory;

        private bool _isTaken;
        private AMinigame _activeMinigame;

        public int GetCount() => 1;

        public bool IsInteractable() => ItemData && !_isTaken && !_activeMinigame;

        public void Interact(object _)
        {
            if (!IsInteractable())
                return;

            if (minigame && itemToSkipMinigame && !_shelterInventory.IsItemInInventory(itemToSkipMinigame))
            {
                var game = MinigamesResolver.Instance.GetMinigame(minigameIndex);

                if (!game || game.IsPlaying())
                    return;

                _activeMinigame = game;
                _activeMinigame.OnMinigameEnded += OnMinigameEnded;
                _activeMinigame.StartMinigame();
                return;
            }

            OnTake();
        }

        private void OnMinigameEnded(bool isWin)
        {
            if (_activeMinigame)
                _activeMinigame.OnMinigameEnded -= OnMinigameEnded;

            _activeMinigame = null;

            if (isWin)
                OnTake();
        }

        private void Start() => GameInstaller.InjectGameObject(gameObject);

        private void OnTake()
        {
            if (_isTaken)
                return;

            _isTaken = true;
            var blueprint = _blueprintLootSystem.TryTakeBlueprint(this);
            _inventory.AddItem(ItemData, GetCount());

            if (blueprint)
                _inventory.AddItem(blueprint);

            Destroy(rootObject ? rootObject : gameObject);
        }

        private void OnDestroy()
        {
            _blueprintLootSystem?.UnregisterLoot(this);

            if (_activeMinigame)
                _activeMinigame.OnMinigameEnded -= OnMinigameEnded;
        }
    }
}