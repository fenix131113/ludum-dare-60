using GameAssembly.Core;
using GameAssembly.Player;
using UnityEngine;
using VContainer;

namespace GameAssembly.ItemsSystem
{
    public class TakeableItem : MonoBehaviour, IInteractable
    {
        [field: SerializeField] public ItemDataSO ItemData { get; private set; }
        [SerializeField] private GameObject rootObject;

        [Inject] private PlayerInventory _inventory;
        
        public int GetCount() => 1;
        
        public void Interact(object _)
        {
            OnTake();
        }
        
        private void Start() => GameInstaller.InjectGameObject(gameObject);
        
        private  void OnTake()
        {
            _inventory.AddItem(ItemData, GetCount());
            Destroy(rootObject);
        }
    }
}