using UnityEngine;

namespace GameAssembly.LootLocation.Generation
{
    public class LocationGenerator : MonoBehaviour
    {
        [SerializeField] private Transform TEMP_SpawnPoint;
        
        public Transform GetEnterPoint() => TEMP_SpawnPoint;
    }
}