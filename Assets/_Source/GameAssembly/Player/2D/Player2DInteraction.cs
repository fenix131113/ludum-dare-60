using System.Collections.Generic;
using GameAssembly.Core;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;

namespace GameAssembly.Player._2D
{
    public class Player2DInteraction : MonoBehaviour
    {
        [SerializeField] private LayerMask interactionLayers;
        [SerializeField] private GameObject interactionHelper;

        [Inject] private PlayerInput _input;
        [Inject] private PlayerState _playerState;

        private readonly List<Transform> _currentInteractions = new();
        private Transform _currentTarget;

        private void Start()
        {
            _input.OnSecondInteractPerformed += OnSecondInteractPerformed;
        }

        private void Update()
        {
            if (!_playerState.IsInTwoD() || _playerState.IsSecondInteractionBlocked() || _currentInteractions.Count == 0)
            {
                ClearCurrentTarget();
                return;
            }

            if (_currentTarget && !_currentInteractions.Contains(_currentTarget))
                ClearCurrentTarget();

            var selectedTarget = 0;
            var currentDistance = float.MaxValue;

            for (var i = 0; i < _currentInteractions.Count; i++)
            {
                var distance = Vector3.Distance(transform.position, _currentInteractions[i].position);
                if (distance >= currentDistance)
                    continue;

                currentDistance = distance;
                selectedTarget = i;
            }

            SetCurrentTarget(_currentInteractions[selectedTarget]);
        }

        private void OnDestroy()
        {
            _input.OnSecondInteractPerformed -= OnSecondInteractPerformed;
        }

        private void OnSecondInteractPerformed()
        {
            if (!_currentTarget || _playerState.IsSecondInteractionBlocked())
                return;

            _currentTarget.GetComponent<IInteractable>().Interact(this);
        }

        private void SetCurrentTarget(Transform target)
        {
            interactionHelper.SetActive(true);
            _currentTarget = target;
        }

        private void ClearCurrentTarget()
        {
            interactionHelper.SetActive(false);
            _currentTarget = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!LayerService.IsLayerEquals(other.gameObject.layer, interactionLayers) ||
                !other.TryGetComponent<IInteractable>(out var interactable) || !interactable.IsInteractable() ||
                _currentInteractions.Contains(other.transform))
                return;

            _currentInteractions.Add(other.transform);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!LayerService.IsLayerEquals(other.gameObject.layer, interactionLayers) ||
                !_currentInteractions.Contains(other.transform))
                return;
            
            if(_currentTarget == other.transform)
                ClearCurrentTarget();
            
            _currentInteractions.Remove(other.transform);
        }
    }
}
