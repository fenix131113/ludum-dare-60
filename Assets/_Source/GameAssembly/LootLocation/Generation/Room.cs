using System.Collections.Generic;
using UnityEngine;

namespace GameAssembly.LootLocation.Generation
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private List<Door> doors;
        [SerializeField] private Door exitDoor;
        [SerializeField] private Transform enterPoint;

        public Transform GetEnterPoint() => enterPoint;

        public Door GetDoorAt(int index)
        {
            if (index >= 0 && index < doors.Count)
            {
                return doors[index];
            }
            Debug.LogError($"Index {index} is out of bounds for doors in {name}!");
            return null;
        }
        
        public void InitializeConnectedRooms(Door previousRoomDoor, params Room[] rooms)
        {
            if (rooms.Length > doors.Count)
            {
                Debug.LogError($"There are more rooms than doors in \"{name}\" room!");
                return;
            }
            
            if (exitDoor && previousRoomDoor)
            {
                exitDoor.InitDoor(previousRoomDoor.GetSpawnPoint());
            }
            else if (exitDoor && !previousRoomDoor)
            {
                Debug.LogWarning($"Room {name} has exitDoor, but previousRoomDoor is null!");
            }
            
            for (var index = 0; index < rooms.Length; index++)
            {
                var room = rooms[index];
                if (room && doors[index])
                {
                    doors[index].InitDoor(room.GetEnterPoint());
                }
            }
        }
    }
}