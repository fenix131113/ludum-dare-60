using System;
using UnityEngine;

namespace GameAssembly.MainMenu
{
    public class ThreeDButton : MonoBehaviour
    {
        public event Action OnButtonClicked;
        
        private void OnMouseDown() => OnButtonClicked?.Invoke();
    }
}