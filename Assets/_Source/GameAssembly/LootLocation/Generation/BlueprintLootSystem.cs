using System.Collections.Generic;
using System.Linq;
using GameAssembly.ItemsSystem;
using GameAssembly.ShelterSystem.CraftSystem;
using UnityEngine;
using VContainer;

namespace GameAssembly.LootLocation.Generation
{
    public class BlueprintLootSystem
    {
        [Inject] private CraftView _craftView;

        private readonly HashSet<TakeableItem> _remainingLoot = new();
        private IReadOnlyList<BlueprintItemDataSO> _blueprints = new BlueprintItemDataSO[0];
        private float _dropChance;
        private bool _blueprintDropped;

        public void BeginLocation(IReadOnlyList<BlueprintItemDataSO> blueprints, float dropChance)
        {
            _remainingLoot.Clear();
            _blueprints = blueprints ?? new BlueprintItemDataSO[0];
            _dropChance = Mathf.Clamp01(dropChance);
            _blueprintDropped = false;
        }

        public void RegisterLoot(TakeableItem item)
        {
            if (item && item.ItemData && item.ItemData is not BlueprintItemDataSO)
                _remainingLoot.Add(item);
        }

        public void UnregisterLoot(TakeableItem item) => _remainingLoot.Remove(item);

        public BlueprintItemDataSO TryTakeBlueprint(TakeableItem item)
        {
            if (!_remainingLoot.Remove(item) || _blueprintDropped)
                return null;

            _remainingLoot.RemoveWhere(loot => !loot);

            var availableBlueprints = _blueprints.Where(blueprint => blueprint && blueprint.Craft &&
                    _craftView.IsCraftBlocked(blueprint.Craft))
                .GroupBy(blueprint => blueprint.Craft)
                .Select(group => group.First())
                .ToArray();

            if (availableBlueprints.Length == 0)
                return null;

            if (_remainingLoot.Count > 0 && _dropChance < 1f && Random.value >= _dropChance)
                return null;

            _blueprintDropped = true;
            return availableBlueprints[Random.Range(0, availableBlueprints.Length)];
        }
    }
}
