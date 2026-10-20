using System.Collections.Generic;
using System.Linq;
using GameAssembly.ItemsSystem;
using UnityEngine;

namespace GameAssembly.ShelterSystem.CraftSystem
{
    public class CraftWorkspace : MonoBehaviour
    {
        [Header("Recipe Association")]
        [SerializeField] private CraftItemSO craftItem;

        [Header("Camera & Visuals")]
        [SerializeField] private Transform closeUpCameraPoint;
        [SerializeField] private GameObject silhouette;

        [Header("Slots & Spawns")]
        [SerializeField] private CraftSlot[] slots;
        [SerializeField] private Transform[] itemSpawnPoints;
        [SerializeField] private List<CraftDraggableItem> prePlacedItems = new();

        private readonly List<CraftDraggableItem> _spawnedItems = new();
        private readonly List<CraftDraggableItem> _activeItems = new();

        public CraftItemSO CraftItem => craftItem;
        public Transform CloseUpCameraPoint => closeUpCameraPoint;
        public IReadOnlyList<CraftSlot> Slots => slots;
        public IReadOnlyList<CraftDraggableItem> ActiveItems => _activeItems;

        public bool IsCompleted => slots != null && slots.Length > 0 && slots.All(s => s.IsCorrectlyFilled);

        private void Awake()
        {
            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<CraftSlot>(true);

            if (prePlacedItems == null || prePlacedItems.Count == 0)
            {
                prePlacedItems = GetComponentsInChildren<CraftDraggableItem>(true).ToList();
            }
        }

        public void SetupWorkspace()
        {
            gameObject.SetActive(true);

            if (silhouette != null)
                silhouette.SetActive(true);

            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<CraftSlot>(true);

            foreach (var slot in slots)
            {
                slot.ResetSlot();
            }

            _activeItems.Clear();

            // Case 1: Pre-placed items exist in the workspace scene hierarchy
            if (prePlacedItems != null && prePlacedItems.Count > 0)
            {
                foreach (var item in prePlacedItems)
                {
                    item.gameObject.SetActive(true);
                    item.ResetItem();
                    _activeItems.Add(item);
                }
            }
            // Case 2: Dynamically spawn items from requirements
            else if (craftItem != null && craftItem.CraftsRequirements != null)
            {
                SpawnRequiredItems();
            }
        }

        public void ClearWorkspace()
        {
            foreach (var slot in slots)
            {
                slot.ResetSlot();
            }

            foreach (var spawned in _spawnedItems)
            {
                if (spawned != null)
                    Destroy(spawned.gameObject);
            }
            _spawnedItems.Clear();

            foreach (var prePlaced in prePlacedItems)
            {
                if (prePlaced != null)
                {
                    prePlaced.ResetItem();
                    prePlaced.gameObject.SetActive(false);
                }
            }

            _activeItems.Clear();

            if (silhouette != null)
                silhouette.SetActive(false);

            gameObject.SetActive(false);
        }

        private void SpawnRequiredItems()
        {
            int spawnIndex = 0;

            foreach (var requirement in craftItem.CraftsRequirements)
            {
                for (int i = 0; i < requirement.Count; i++)
                {
                    Vector3 spawnPos = transform.position;
                    Quaternion spawnRot = Quaternion.identity;

                    if (itemSpawnPoints != null && spawnIndex < itemSpawnPoints.Length && itemSpawnPoints[spawnIndex] != null)
                    {
                        spawnPos = itemSpawnPoints[spawnIndex].position;
                        spawnRot = itemSpawnPoints[spawnIndex].rotation;
                    }
                    else
                    {
                        // Default fallback spacing below workspace center
                        spawnPos = transform.position + transform.right * (spawnIndex * 0.4f - 0.6f) - transform.forward * 0.35f;
                    }

                    GameObject prefab = null;
                    if (requirement.ItemData is MaterialItemDataSO materialItem)
                    {
                        prefab = materialItem.WorkbenchPrefab;
                    }

                    GameObject instance;
                    if (prefab != null)
                    {
                        instance = Instantiate(prefab, spawnPos, spawnRot, transform);
                    }
                    else
                    {
                        // Fallback primitive if no workbench prefab assigned yet
                        instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        instance.transform.SetParent(transform);
                        instance.transform.position = spawnPos;
                        instance.transform.rotation = spawnRot;
                        instance.transform.localScale = Vector3.one * 0.2f;
                    }

                    instance.name = $"{requirement.ItemData.ItemName}_{i + 1}";

                    if (!instance.TryGetComponent(out CraftDraggableItem draggable))
                    {
                        draggable = instance.AddComponent<CraftDraggableItem>();
                    }

                    draggable.Initialize(requirement.ItemData, spawnPos, spawnRot);
                    _spawnedItems.Add(draggable);
                    _activeItems.Add(draggable);

                    spawnIndex++;
                }
            }
        }
    }
}
