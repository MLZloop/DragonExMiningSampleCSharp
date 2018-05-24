using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonExMiningSampleCSharp.Apis.Entity
{
    class ProfitsEntity
    {
        private UserAmounts baseA;
        private UserAmounts baseB;
        private UserAmounts currentA;
        private UserAmounts currentB;

        public decimal BaseBaseAmount
        {
            get
            {
                if (baseB != null && baseA != null)
                {
                    return baseA.BaseAmount + baseB.BaseAmount;
                }
                else
                {
                    return 0.0m;
                }
            }
        }

        public decimal BaseCoinAmount
        {
            get
            {
                if (baseB != null && baseA != null)
                {
                    return baseA.CoinAmount + baseB.CoinAmount;
                }
                else
                {
                    return 0.0m;
                }
            }
        }

        public decimal CurrentBaseAmount
        {
            get
            {
                return currentA.BaseAmount + currentB.BaseAmount;
            }
        }

        public decimal CurrentCoinAmount
        {
            get
            {
                return currentA.CoinAmount + currentB.CoinAmount;
            }
        }

        public decimal ProfitsBaseAmount
        {
            get
            {
                if (baseB != null && baseA != null && currentB != null && currentA != null)
                {
                    return CurrentBaseAmount - baseB.BaseAmount - baseA.BaseAmount;
                }
                else
                {
                    return 0.0m;
                }
            }
        }

        public decimal ProfitsCoinAmount
        {
            get
            {
                if (baseB != null && baseA != null && currentB != null && currentA != null)
                {
                    return CurrentCoinAmount - baseB.CoinAmount - baseA.CoinAmount;
                }
                else
                {
                    return 0.0m;
                }
            }
        }

        public void UpdateBase(UserAmounts q, UserAmounts b)
        {
            baseA = new UserAmounts(q.TradeName, q.BaseAmount, q.CoinAmount);
            baseB = new UserAmounts(b.TradeName, b.BaseAmount, b.CoinAmount);
        }

        public void UpdateCurrent(UserAmounts q, UserAmounts b)
        {
            currentA = new UserAmounts(q.TradeName, q.BaseAmount, q.CoinAmount);
            currentB = new UserAmounts(b.TradeName, b.BaseAmount, b.CoinAmount);
        }

    }
}
