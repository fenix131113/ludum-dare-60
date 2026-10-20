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

        private readonly HashSet<Transform> _reservedLootPositions = new();

        public IReadOnlyList<Transform> LootPositions => lootPositions;

        public void ReserveLootPosition(Transform position) => _reservedLootPositions.Add(position);

        private void Start()
        {
            SpawnLoot();
        }

        public void SpawnLoot()
        {
            if (lootPositions == null || lootPositions.Count == 0)
                return;

            foreach (var positionData in lootPositions)
            {
                if (!positionData || _reservedLootPositions.Contains(positionData))
                    continue;

                var selectedLoot = GetRandomItemByWeight(lootWeights);

                if (!selectedLoot)
                    continue;

                if (selectedLoot.LootPrefab)
                {
                    Instantiate(selectedLoot.LootPrefab, positionData.position,
                        positionData.rotation).transform.parent = transform;
                }
            }
        }

        private MaterialItemDataSO GetRandomItemByWeight(List<LootWeightData> pool)
        {
            var totalWeight = pool.Where(lootData => lootData.Item && lootData.Weight > 0)
                .Sum(lootData => lootData.Weight);

            if (totalWeight <= 0f)
                return null;

            var randomValue = Random.Range(0f, totalWeight);
            var currentWeightSum = 0f;

            foreach (var lootData in pool.Where(lootData => lootData.Item != null && !(lootData.Weight <= 0)))
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
