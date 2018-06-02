using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DragonExMiningSampleCSharp.Apis.Common.Enums;

namespace DragonExMiningSampleCSharp.Apis.Config
{
    class TestConfigTool
    {
        public static bool TradeBaseToCoinFailed = false;

        public static bool TradeCoinToBaseFailed = false;

        public static bool CheckAOrderFailed = false;

        public static bool CheckBOrderFailed = false;

        public static bool CancelAOrderFailed = false;

        public static bool CancelBOrderFailed = false;
    }
}
