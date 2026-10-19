using System;

namespace GameAssembly.Player
{
    public class PlayerBank
    {
        public int Money { get; private set; }
        
        public event Action<int, int> OnMoneyChanged;

        public void AddMoney(int amount)
        {
            var temp = Money;
            Money += amount;
            OnMoneyChanged?.Invoke(temp, Money);
        }

        public bool TryRemoveMoney(int amount)
        {
            if (Money < amount)
                return false;
            
            var temp = Money;
            Money -= amount;
            OnMoneyChanged?.Invoke(temp, Money);
            return true;
        }
    }
}