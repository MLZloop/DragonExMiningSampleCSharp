using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonExMiningSampleCSharp.Apis.Entity
{
    class TradeEntity
    {
        /// <summary>
        /// Whether Success
        /// </summary>
        public bool Success
        {
            get;
            set;
        }

        /// <summary>
        /// Received
        /// </summary>
        public decimal Received
        {
            get;
            set;
        }

        /// <summary>
        /// Remains
        /// </summary>
        public decimal Remains
        {
            get;
            set;
        }

        /// <summary>
        /// Order Id
        /// </summary>
        public string OrderId
        {
            get;
            set;
        }
    }
}
