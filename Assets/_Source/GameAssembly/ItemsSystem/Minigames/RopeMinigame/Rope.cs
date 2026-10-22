using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GameAssembly.ItemsSystem.Minigames.RopeMinigame
{
    public class Rope : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image img;
        
        public event Action<Rope> OnClicked;
        
        private bool _isValid;

        public void Initialize(Sprite sprite, bool isValid = false)
        {
            img.sprite = sprite;
            _isValid = isValid;
            gameObject.SetActive(true);
        }

        public bool IsValid() =>  _isValid;
        
        public void OnPointerClick(PointerEventData eventData) => OnClicked?.Invoke(this);
    }
}