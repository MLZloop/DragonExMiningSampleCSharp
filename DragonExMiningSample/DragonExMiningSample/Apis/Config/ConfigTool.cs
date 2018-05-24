using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DragonExMiningSampleCSharp.Apis.Common.Enums;

namespace DragonExMiningSampleCSharp.Apis.Config
{
    class ConfigTool
    {
        /// <summary>
        /// Trade minimum amount
        /// </summary>
        public static decimal TradeMinAmount = 0.001m;

        /// <summary>
        /// Use Current Ask/Bid Index
        /// </summary>
        public static int UseCurrentAskBidIndex = 1;
        
        /// <summary>
        /// Ticker Inteval for getting information
        /// </summary>
        public static int TickerInterval = 1000;

        /// <summary>
        /// Retry Count when trade failed
        /// </summary>
        public static int TradeFailedRetryCount = 1;

        /// <summary>
        /// Ignore error count
        /// </summary>
        public static int IgnoreErrorCount = 5;

        /// <summary>
        /// One trade amount
        /// </summary>
        public static decimal OneTradeAmount = 0;

        /// <summary>
        /// User info auto get interval
        /// </summary>
        public static int UserInfoGetInterval = 12000;

        /// <summary>
        /// Trade Pair Dictionary
        /// </summary>
        public static Dictionary<string, int> TradePairDict = new Dictionary<string, int>();

        /// <summary>
        /// Coin Dictionary
        /// </summary>
        public static Dictionary<string, int> CoinDict = new Dictionary<string, int>();

        /// <summary>
        /// Current Pair
        /// </summary>
        public static KeyValuePair<string, int> CurrentPair = new KeyValuePair<string, int>();

        /// <summary>
        /// Current Base
        /// </summary>
        public static string CurrentBase = "";

        /// <summary>
        /// Current Coin
        /// </summary>
        public static string CurrentCoin = "";

        /// <summary>
        /// Minimum trade usdt amount
        /// </summary>
        public static decimal MinimumTradeUsdtAmount = 1.0m;

        /// <summary>
        /// Trade Fee
        /// </summary>
        public static decimal TradeFee = 0.002m;

        /// <summary>
        /// Try count When sell succeed but buy failed
        /// </summary>
        public static int TradeTryCountWhenSellSucceed = 3;

        /// <summary>
        /// Try count for confirm whether trade succeed
        /// </summary>
        public static int ConfirmTryCountForTradeSucceed = 5;

        /// <summary>
        /// Digits
        /// </summary>
        public static int Digits = 8;

        /// <summary>
        /// Run Mode
        /// </summary>
        public static RunMode RunMode = (RunMode)Enum.Parse(typeof(RunMode), Properties.Settings.Default.RunMode);
    }
}
