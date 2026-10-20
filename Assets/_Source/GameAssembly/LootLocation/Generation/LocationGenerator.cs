using System.Collections.Generic;
using GameAssembly.Core;
using GameAssembly.ItemsSystem;
using GameAssembly.ShelterSystem.CraftSystem;
using UnityEngine;
using VContainer;

namespace GameAssembly.LootLocation.Generation
{
    public class LocationGenerator : MonoBehaviour
    {
        [SerializeField] private Transform startRoomPoint;
        [SerializeField] private Room[] enterRoomsPrefabs;
        [SerializeField] private Room[] bigRoomsPrefabs;
        [SerializeField] private Room[] mediumRoomsPrefabs;
        [SerializeField] private Room[] transitionRoomsPrefabs;
        [SerializeField] private float roomDistance;

        [SerializeField] private List<BlueprintItemDataSO> blueprints = new();

        [Inject] private CraftView _craftView;

        private readonly List<Room> _mediumSpawned = new();
        private readonly List<Room> _transitionSpawned = new();
        private readonly List<RoomLootSpawner> _lootSpawners = new();
        private Room _bigSpawned;
        private Room _startSpawned;

        private float _currentSpawnOffset;

        public Transform GetEnterPoint() => _startSpawned.GetEnterPoint();

        public void GenerateLocation()
        {
            ClearRooms();
            _currentSpawnOffset = 0;

            var selectedStartRoom = enterRoomsPrefabs[Random.Range(0, enterRoomsPrefabs.Length)];
            _startSpawned = SpawnRoom(selectedStartRoom);

            var selectedBigRoom = bigRoomsPrefabs[Random.Range(0, bigRoomsPrefabs.Length)];
            _bigSpawned = SpawnRoom(selectedBigRoom);

            for (var i = 0; i < 2; i++)
            {
                var transition =
                    transitionRoomsPrefabs
                        [Random.Range(0, transitionRoomsPrefabs.Length)];
                var spawned = SpawnRoom(transition);
                _transitionSpawned.Add(spawned);
            }

            for (var i = 0; i < 2; i++)
            {
                var medium = mediumRoomsPrefabs[Random.Range(0, mediumRoomsPrefabs.Length)];
                var spawned = SpawnRoom(medium);
                _mediumSpawned.Add(spawned);
            }
            
            _startSpawned.InitializeConnectedRooms(null, _bigSpawned);
            var doorToBigRoom = _startSpawned.GetDoorAt(0);
            
            _bigSpawned.InitializeConnectedRooms(doorToBigRoom, _transitionSpawned.ToArray());
            
            for (var i = 0; i < _transitionSpawned.Count; i++)
            {
                var doorToTransition = _bigSpawned.GetDoorAt(i);
                _transitionSpawned[i].InitializeConnectedRooms(doorToTransition, _mediumSpawned[i]);
            }

            for (var i = 0; i < _mediumSpawned.Count; i++)
            {
                var doorToMedium = _transitionSpawned[i].GetDoorAt(0);
                
                _mediumSpawned[i].InitializeConnectedRooms(doorToMedium);
            }

            SpawnBlueprint();
        }

        private void SpawnBlueprint()
        {
            var availableBlueprints = blueprints.FindAll(blueprint => blueprint && blueprint.Craft &&
                blueprint.LootPrefab && _craftView.IsCraftBlocked(blueprint.Craft));

            if (availableBlueprints.Count == 0)
                return;

            var positions = new List<Transform>();

            foreach (var spawner in _lootSpawners)
            {
                foreach (var position in spawner.LootPositions)
                {
                    if (position && !positions.Contains(position))
                        positions.Add(position);
                }
            }

            if (positions.Count == 0)
            {
                Debug.LogWarning("[LocationGenerator] No loot positions available for a blueprint!");
                return;
            }

            var selectedBlueprint = availableBlueprints[Random.Range(0, availableBlueprints.Count)];
            var selectedPosition = positions[Random.Range(0, positions.Count)];

            foreach (var spawner in _lootSpawners)
                spawner.ReserveLootPosition(selectedPosition);

            Instantiate(selectedBlueprint.LootPrefab, selectedPosition.position, selectedPosition.rotation,
                selectedPosition);
        }


        private void ClearRooms()
        {
            _lootSpawners.Clear();

            _mediumSpawned.ForEach(x => Destroy(x.gameObject));
            _mediumSpawned.Clear();

            _transitionSpawned.ForEach(x => Destroy(x.gameObject));
            _transitionSpawned.Clear();

            if(_bigSpawned)
                Destroy(_bigSpawned.gameObject);
            
            if(_startSpawned)
                Destroy(_startSpawned.gameObject);
        }

        private Room SpawnRoom(Room room)
        {
            var spawned = Instantiate(room, startRoomPoint.position + Vector3.forward * _currentSpawnOffset,
                Quaternion.identity);
            GameInstaller.InjectGameObject(spawned.gameObject);
            _lootSpawners.AddRange(spawned.GetComponentsInChildren<RoomLootSpawner>());

            _currentSpawnOffset += roomDistance;

            return spawned;
        }
    }
}
