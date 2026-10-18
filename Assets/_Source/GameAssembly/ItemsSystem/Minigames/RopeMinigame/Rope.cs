using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameAssembly.ItemsSystem.Minigames.RopeMinigame
{
    public class Rope : MonoBehaviour, IPointerClickHandler
    {
        public event Action<Rope> OnClicked;
        
        private bool _isValid;

        public void Initialize(bool isValid = false)
        {
            _isValid = isValid;
            gameObject.SetActive(true);
        }

        public bool IsValid() =>  _isValid;
        
        public void OnPointerClick(PointerEventData eventData) => OnClicked?.Invoke(this);
    }
}