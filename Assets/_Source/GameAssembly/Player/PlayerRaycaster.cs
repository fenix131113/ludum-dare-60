using GameAssembly.Player.Data;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;

namespace GameAssembly.Player
{
    public class PlayerRaycaster : MonoBehaviour
    {
        [SerializeField] private LayerMask rayLayers;
        [SerializeField] private GameObject interactHelper;
        
        [Inject] private PlayerInput _input;
        [Inject] private PlayerSettings _playerSettings;

        private GameObject _target;

        private void Start()
        {
            Bind();
        }

        private void Update()
        {
            var ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out var hit, _playerSettings.RayLenght, rayLayers))
            {
                if (hit.transform.TryGetComponent(out IInteractable _))
                    ActiveTarget(hit.transform.gameObject);
                else
                    ResetTarget();
            }
            else
                ResetTarget();
        }

        private void ResetTarget()
        {
            _target = null;
            
            if(interactHelper)
                interactHelper.SetActive(false);
        }

        private void ActiveTarget(GameObject target)
        {
            _target = target;
            
            if(interactHelper)
                interactHelper?.SetActive(true);
        }

        private void OnInteractPerformed()
        {
            if (!_target)
                return;
            
            if(_target.TryGetComponent(out IInteractable interactable))
                interactable.Interact(this);
        }
        
        private void OnDestroy() => Expose();

        private void Bind()
        {
            _input.OnInteractPerformed += OnInteractPerformed;
        }

        private void Expose()
        {
            _input.OnInteractPerformed -= OnInteractPerformed;
        }
    }
}