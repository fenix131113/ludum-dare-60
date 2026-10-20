using System.Collections.Generic;
using GameAssembly.Core;
using GameAssembly.ItemsSystem;
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
        [SerializeField, Range(0f, 1f)] private float blueprintDropChance = 0.1f;

        [SerializeField] private List<LootWeightData> storyLootWeights = new();

        [Inject] private BlueprintLootSystem _blueprintLootSystem;
        [Inject] private StoryLootSystem _storyLootSystem;

        private readonly List<Room> _mediumSpawned = new();
        private readonly List<Room> _transitionSpawned = new();
        private Room _bigSpawned;
        private Room _startSpawned;

        private float _currentSpawnOffset;

        public Transform GetEnterPoint() => _startSpawned.GetEnterPoint();

        public void GenerateLocation()
        {
            ClearRooms();
            _storyLootSystem.BeginLocation();
            _blueprintLootSystem.BeginLocation(blueprints, blueprintDropChance);
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
        }

        private void ClearRooms()
        {
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
            var spawners = spawned.GetComponentsInChildren<RoomLootSpawner>();

            foreach (var spawner in spawners)
            {
                spawner.InitializeStoryLoot(_storyLootSystem, storyLootWeights);
                spawner.InitializeBlueprintLoot(_blueprintLootSystem);
                spawner.SpawnLoot();
            }

            _currentSpawnOffset += roomDistance;

            return spawned;
        }
    }
}
