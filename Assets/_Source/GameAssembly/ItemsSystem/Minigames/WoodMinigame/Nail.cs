using System;
using GameAssembly.Player.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace GameAssembly.ItemsSystem.Minigames.WoodMinigame
{
    public class Nail : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Vector2 scrollVector;
        [SerializeField] private float needScrollValue;
        [SerializeField] private float scrollStrength;
        
        [Inject] private PlayerInput _input;

        private Vector2 _startPosition;
        private bool _isHolding;
        private float _currentScroll;
        
        public event Action<Nail> OnNailGet;
        
        public float Progress => _currentScroll /  needScrollValue;
        
        public void Initialize()
        {
            if(_startPosition == Vector2.zero)
                _startPosition = transform.localPosition;
            
            transform.localPosition = _startPosition;
            _currentScroll = 0;
            gameObject.SetActive(true);
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _isHolding = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isHolding = false;
        }
        
        private void Update()
        {
            if(!_isHolding)
                return;
            
            ScrollNail();
        }

        private void ScrollNail()
        {
            var scroll = _input.ReadScrollVector();
            
            if(scroll.y > 0)
                return;
            
            _currentScroll -= scroll.y * scrollStrength;
            transform.localPosition = Vector2.Lerp(_startPosition, _startPosition + scrollVector, Progress);

            if (_currentScroll >= needScrollValue)
                GetNail();
        }

        private void GetNail()
        {
            OnNailGet?.Invoke(this);
            gameObject.SetActive(false);
            _isHolding = false;
        }
    }
}