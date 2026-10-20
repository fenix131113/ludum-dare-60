using GameAssembly.Core;
using GameAssembly.ItemsSystem.Minigames;
using GameAssembly.Player;
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

        [Inject] private PlayerInventory _inventory;

        public int GetCount() => 1;

        public bool IsInteractable() => true;

        public void Interact(object _)
        {
            if (minigame)
            {
                var game = MinigamesResolver.Instance.GetMinigame(minigameIndex);
                game.OnMinigameEnded += OnMinigameEnded;
                game.StartMinigame();
                return;
            }

            OnTake();
        }

        private void OnMinigameEnded(bool isWin)
        {
            var game = MinigamesResolver.Instance.GetMinigame(minigameIndex);
            game.OnMinigameEnded -= OnMinigameEnded;
            
            if (isWin)
                OnTake();
        }

        private void Start() => GameInstaller.InjectGameObject(gameObject);

        private void OnTake()
        {
            _inventory.AddItem(ItemData, GetCount());
            Destroy(rootObject);
        }

        private void OnDestroy()
        {
            if (!minigame)
                return;

            var game = MinigamesResolver.Instance?.GetMinigame(minigameIndex);

            if (game && game.IsPlaying())
                game.OnMinigameEnded -= OnMinigameEnded;
        }
    }
}
