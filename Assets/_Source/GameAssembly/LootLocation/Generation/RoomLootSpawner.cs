using System;
using System.Collections.Generic;
using System.Linq;
using GameAssembly.ItemsSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameAssembly.LootLocation.Generation
{
    public class RoomLootSpawner : MonoBehaviour
    {
        [SerializeField] private List<Transform> lootPositions = new();
        [SerializeField] private List<LootWeightData> lootWeights;

        private StoryLootSystem _storyLootSystem;
        private BlueprintLootSystem _blueprintLootSystem;
        private bool _hasSpawned;
        private IReadOnlyList<LootWeightData> _storyLootWeights = Array.Empty<LootWeightData>();

        public void InitializeStoryLoot(StoryLootSystem storyLootSystem, IReadOnlyList<LootWeightData> weights)
        {
            _storyLootSystem = storyLootSystem;
            _storyLootWeights = weights;
        }

        public void InitializeBlueprintLoot(BlueprintLootSystem blueprintLootSystem) => _blueprintLootSystem = blueprintLootSystem;

        private void Start()
        {
            SpawnLoot();
        }

        public void SpawnLoot()
        {
            if (_hasSpawned || lootPositions == null || lootPositions.Count == 0)
                return;

            _hasSpawned = true;

            foreach (var positionData in lootPositions)
            {
                if (!positionData)
                    continue;

                var selectedLoot = GetRandomItemByWeight(lootWeights);

                if (!selectedLoot)
                    continue;

                if (selectedLoot.LootPrefab)
                {
                    var spawned = Instantiate(selectedLoot.LootPrefab, positionData.position, selectedLoot.LootPrefab.transform.rotation, transform);

                    if (_blueprintLootSystem != null)
                    {
                        foreach (var item in spawned.GetComponentsInChildren<TakeableItem>())
                            _blueprintLootSystem.RegisterLoot(item);
                    }

                    if (selectedLoot.StoryItem)
                        _storyLootSystem.RegisterSpawned(selectedLoot);
                }
            }
        }

        private MaterialItemDataSO GetRandomItemByWeight(List<LootWeightData> pool)
        {
            var normalLoot = pool ?? new List<LootWeightData>();
            var storyLoot = _storyLootWeights ?? Array.Empty<LootWeightData>();
            var availableLoot = normalLoot.Where(lootData => lootData.Item && !lootData.Item.StoryItem && lootData.Item.LootPrefab && lootData.Weight > 0)
                .Concat(storyLoot.Where(lootData => lootData.Item && lootData.Item.StoryItem && lootData.Item.LootPrefab &&
                    lootData.Weight > 0 && _storyLootSystem != null && _storyLootSystem.CanSpawn(lootData.Item)))
                .ToArray();
            var totalWeight = availableLoot
                .Sum(lootData => lootData.Weight);

            if (totalWeight <= 0f)
                return null;

            var randomValue = Random.Range(0f, totalWeight);
            var currentWeightSum = 0f;

            foreach (var lootData in availableLoot)
            {
                currentWeightSum += lootData.Weight;
                if (randomValue <= currentWeightSum)
                    return lootData.Item;
            }

            return null;
        }
    }

    [Serializable]
    public struct LootWeightData
    {
        [SerializeField] private MaterialItemDataSO item;
        [SerializeField] private float weight;

        public MaterialItemDataSO Item => item;
        public float Weight => weight;
    }
}
