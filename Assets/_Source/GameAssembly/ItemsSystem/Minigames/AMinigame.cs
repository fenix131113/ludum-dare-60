using System;
using UnityEngine;

namespace GameAssembly.ItemsSystem.Minigames
{
    public abstract class AMinigame : MonoBehaviour
    {
        public abstract event Action<bool> OnMinigameEnded;
        
        public abstract void StartMinigame();
        public abstract void ResetMinigame();
        public abstract void WinMinigame();
        public abstract void LooseMinigame();
        public abstract bool IsPlaying();
    }
}