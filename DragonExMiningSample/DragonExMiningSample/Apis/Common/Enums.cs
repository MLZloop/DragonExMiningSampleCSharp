using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DragonExMiningSampleCSharp.Apis.Common
{
    public class Enums
    {
        /// <summary>
        /// Log Levels
        /// </summary>
        public enum LogLevels
        {
            INFO = 0,
            TRACE = 1,
            ERROR = 2
        }

        /// <summary>
        /// Trade Type
        /// </summary>
        public enum TradeTypes
        {
            BUY,
            SELL
        }

        /// <summary>
        /// Trade Side
        /// </summary>
        public enum TradeSide
        {
            A_TO_B,
            B_TO_A,
            BOTH_A_B_A_FIRST,
            BOTH_A_B_B_FIRST
        }

        /// <summary>
        /// Trade method
        /// </summary>
        public enum TradeMethod
        {
            A_TO_B,
            B_TO_A
        }

        /// <summary>
        /// Run mode
        /// </summary>
        public enum RunMode
        {
            NORMAL,
            TEST
        }
    }
}