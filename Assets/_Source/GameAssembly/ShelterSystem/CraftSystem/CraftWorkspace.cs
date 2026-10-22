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

        [Header("Slots & Items")]
        [SerializeField] private CraftSlot[] slots;
        [SerializeField] private List<CraftDraggableItem> prePlacedItems = new();

        [Header("Automatic Setup")]
        [SerializeField] private bool autoSetup;
        [SerializeField] private Transform[] assemblyPoints;
        [SerializeField] private CraftSlot slotPrefab;
        [SerializeField, Min(0.01f)] private float slotSnapRadius = 0.35f;

        private readonly List<CraftDraggableItem> _spawnedItems = new();
        private readonly List<CraftDraggableItem> _activeItems = new();
        private readonly List<CraftSlot> _spawnedSlots = new();

        public CraftItemSO CraftItem => craftItem;
        public Transform CloseUpCameraPoint => closeUpCameraPoint;
        public IReadOnlyList<CraftSlot> Slots => slots;
        public IReadOnlyList<CraftDraggableItem> ActiveItems => _activeItems;

        public bool IsCompleted => slots != null && slots.Length > 0 && slots.All(s => s && s.IsCorrectlyFilled);

        private void Awake()
        {
            if (!autoSetup && (slots == null || slots.Length == 0))
                slots = GetComponentsInChildren<CraftSlot>(true);

            if (!autoSetup && (prePlacedItems == null || prePlacedItems.Count == 0))
            {
                prePlacedItems = GetComponentsInChildren<CraftDraggableItem>(true).ToList();
            }
        }

        public bool SetupWorkspace(Transform[] itemSpawnPoints)
        {
            ClearWorkspace();

            if (autoSetup && !ValidateAutomaticSetup(itemSpawnPoints))
                return false;

            gameObject.SetActive(true);

            if (silhouette != null)
                silhouette.SetActive(true);

            if (autoSetup)
            {
                SpawnSlots();
                SpawnRequiredItems(itemSpawnPoints);
                    return true;
            }

            if (slots == null || slots.Length == 0)
                slots = GetComponentsInChildren<CraftSlot>(true);

            foreach (var slot in slots)
            {
                if (slot)
                    slot.ResetSlot();
            }

            _activeItems.Clear();

            // Case 1: Pre-placed items exist in the workspace scene hierarchy
            if (prePlacedItems != null && prePlacedItems.Count > 0)
            {
                foreach (var item in prePlacedItems)
                {
                    if (!item)
                        continue;

                    item.gameObject.SetActive(true);
                    item.ResetItem();
                    _activeItems.Add(item);
                }
            }
            // Case 2: Dynamically spawn items from requirements
            else if (craftItem != null && craftItem.CraftsRequirements != null)
            {
                SpawnRequiredItems(itemSpawnPoints);
            }

            return true;
        }

        public void ClearWorkspace()
        {
            if (slots != null)
            {
                foreach (var slot in slots)
                {
                    if (slot)
                        slot.ResetSlot();
                }
            }

            foreach (var spawned in _spawnedItems)
            {
                if (spawned != null)
                {
                    spawned.gameObject.SetActive(false);
                    Destroy(spawned.gameObject);
                }
            }
            _spawnedItems.Clear();

            foreach (var slot in _spawnedSlots)
            {
                if (slot)
                {
                    slot.gameObject.SetActive(false);
                    Destroy(slot.gameObject);
                }
            }

            if (_spawnedSlots.Count > 0)
                slots = new CraftSlot[0];

            _spawnedSlots.Clear();

            if (prePlacedItems != null)
            {
                foreach (var prePlaced in prePlacedItems)
                {
                    if (prePlaced != null)
                    {
                        prePlaced.gameObject.SetActive(false);
                    }
                }
            }

            _activeItems.Clear();

            if (silhouette != null)
                silhouette.SetActive(false);

            gameObject.SetActive(false);
        }

        private bool ValidateAutomaticSetup(Transform[] itemSpawnPoints)
        {
            if (!craftItem || craftItem.CraftsRequirements == null || craftItem.CraftsRequirements.Count == 0 ||
                craftItem.CraftsRequirements.Any(requirement => requirement == null || !requirement.ItemData || requirement.Count <= 0))
            {
                Debug.LogError($"[CraftWorkspace] Assign a recipe with valid ingredients in {name}!");
                return false;
            }

            var count = craftItem.CraftsRequirements.Sum(requirement => requirement.Count);

            if (!ArePointsValid(itemSpawnPoints, count) || !ArePointsValid(assemblyPoints, count))
            {
                Debug.LogError($"[CraftWorkspace] {craftItem.name} needs {count} different Item Spawn Points on CraftPhysicalGame and {count} different Assembly Points in {name}!");
                return false;
            }

            return true;
        }

        private bool ArePointsValid(Transform[] points, int count) => points != null && points.Length >= count &&
            points.Take(count).All(point => point) && points.Take(count).Distinct().Count() == count;

        private void SpawnSlots()
        {
            var slotIndex = 0;

            foreach (var requirement in craftItem.CraftsRequirements)
            {
                for (var i = 0; i < requirement.Count; i++)
                {
                    var point = assemblyPoints[slotIndex];
                    CraftSlot slot;

                    if (slotPrefab)
                        slot = Instantiate(slotPrefab, point.position, point.rotation, transform);
                    else
                    {
                        var instance = new GameObject("CraftSlot");
                        instance.transform.SetParent(transform, false);
                        instance.transform.SetPositionAndRotation(point.position, point.rotation);
                        slot = instance.AddComponent<CraftSlot>();
                    }

                    slot.name = $"Slot_{requirement.ItemData.ItemName}_{i + 1}";
                    slot.Initialize(requirement.ItemData, point, slotSnapRadius);
                    slot.gameObject.SetActive(true);
                    _spawnedSlots.Add(slot);
                    slotIndex++;
                }
            }

            slots = _spawnedSlots.ToArray();
        }

        private void SpawnRequiredItems(Transform[] itemSpawnPoints)
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
                    if (prefab)
                    {
                        // Spawn points define only where the item appears. Its authored
                        // prefab rotation is part of the item's visual setup.
                        spawnRot = prefab.transform.rotation;
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

                    if (!instance.TryGetComponent<Collider>(out _))
                        instance.AddComponent<BoxCollider>();

                    if (!instance.TryGetComponent(out CraftDraggableItem draggable))
                    {
                        draggable = instance.AddComponent<CraftDraggableItem>();
                    }

                    draggable.Initialize(requirement.ItemData, spawnPos, spawnRot);
                    instance.SetActive(true);
                    _spawnedItems.Add(draggable);
                    _activeItems.Add(draggable);

                    spawnIndex++;
                }
            }
        }
    }
}
