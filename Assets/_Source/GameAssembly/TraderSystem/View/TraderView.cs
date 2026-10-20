using GameAssembly.Player;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;

namespace GameAssembly.TraderSystem.View
{
    public class TraderView : MonoBehaviour
    {
        [SerializeField] protected GameObject traderPanel;
        
        [Inject] protected PlayerState _playerState;
        [Inject] protected PlayerInput _input;

        private bool _isBind;

        public virtual void OpenTraderView()
        {
            traderPanel.SetActive(true);
            _playerState.ChangeBlockSecondInteraction(true);
            _input.OnSecondInteractPerformed += CloseTraderView;
            _isBind = true;
        }

        public virtual void CloseTraderView()
        {
            if(_isBind)
            {
                _input.OnSecondInteractPerformed -= CloseTraderView;
                _isBind = false;
            }

            traderPanel.SetActive(false);
            _playerState.ChangeBlockSecondInteraction(false);
        }

        protected virtual void OnDestroy()
        {
            if(_isBind)
            {
                _input.OnSecondInteractPerformed -= CloseTraderView;
                _isBind = false;
            }
        }
    }
}
