using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonExMiningSampleCSharp.Apis.Entity
{
    class UserAmounts
    {
        /// <summary>
        /// Last updated date
        /// </summary>
        public DateTime UpdatedDate
        {
            get;
            set;
        }

        /// <summary>
        /// Trade Name
        /// </summary>
        public string TradeName
        {
            get;
            set;
        }

        /// <summary>
        /// Base Amount
        /// </summary>
        public decimal BaseAmount
        {
            get;
            set;
        }

        /// <summary>
        /// Coin Amount
        /// </summary>
        public decimal CoinAmount
        {
            get;
            set;
        }

        /// <summary>
        /// Default Constructor
        /// </summary>
        public UserAmounts()
        {

        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="tradeName">Trade Name</param>
        /// <param name="baseAmount">Base Amount</param>
        /// <param name="coinAmount">Coin Amount</param>
        public UserAmounts(string tradeName, decimal baseAmount, decimal coinAmount)
        {
            this.TradeName = tradeName;
            this.BaseAmount = baseAmount;
            this.CoinAmount = coinAmount;
        }
    }
}
