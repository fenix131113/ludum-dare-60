using UnityEngine;

namespace GameAssembly.ItemsSystem.Minigames
{
    public class MinigamesResolver : MonoBehaviour
    {
        private static  MinigamesResolver _instance;
        public static MinigamesResolver Instance => !_instance ? null : _instance;
        
        [SerializeField] private AMinigame[] minigames;

        public AMinigame GetMinigame(int index)
        {
            if (index < minigames.Length)
                return minigames[index];
            
            Debug.LogError("Wrong minigame index!");
            return null;
        }

        private void Awake()
        {
            if(!Instance)
                _instance = this;
        }
    }
}