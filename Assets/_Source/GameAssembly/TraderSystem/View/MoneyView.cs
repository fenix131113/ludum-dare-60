using GameAssembly.Player;
using TMPro;
using UnityEngine;
using VContainer;

namespace GameAssembly.TraderSystem.View
{
    public class MoneyView : MonoBehaviour
    {
        [SerializeField] private TMP_Text moneyLabel;
        
        [Inject] private PlayerBank _playerBank;

        private void Start()
        {
            _playerBank.OnMoneyChanged += Redraw;
            Redraw(_playerBank.Money, _playerBank.Money);
        }

        private void OnDestroy()
        {
            _playerBank.OnMoneyChanged -= Redraw;
        }

        private void Redraw(int oldValue, int newValue) => moneyLabel.text = $"{newValue}$";
    }
}