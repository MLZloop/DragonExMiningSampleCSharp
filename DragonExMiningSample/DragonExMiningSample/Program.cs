using DragonExMiningSampleCSharp.Apis.Config;
using DragonExMiningSampleCSharp.Apis.DragonEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DragonExMiningSampleCSharp
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Dictionary<string, string> symbolCoinMatchDict = new Dictionary<string, string>();
            symbolCoinMatchDict.Add("gxs", "gas");

            var allCoinEle = DragonExPublicApis.Instance.GetAllCoins();

            Dictionary<string, int> coinDict = new Dictionary<string, int>();
            coinDict.Add("usdt", 1);
            var coinPairs = allCoinEle.Elements("data");
            foreach (var item in coinPairs)
            {
                string coin = item.Element("code").Value;
                if (symbolCoinMatchDict.ContainsKey(coin))
                {
                    coin = symbolCoinMatchDict[coin];
                }
                if (!coinDict.ContainsKey(coin))
                {
                    coinDict.Add(coin, int.Parse(item.Element("coin_id").Value));
                }
            }
            ConfigTool.CoinDict = coinDict;

            var allSymbolEle = DragonExPublicApis.Instance.GetAllSymbols();
            Dictionary<string, int> symbolDict = new Dictionary<string, int>();
            var symbols = allSymbolEle.Elements("data");
            foreach (var item in symbols)
            {
                string pair = item.Element("symbol").Value;
                if (!symbolDict.ContainsKey(pair))
                {
                    symbolDict.Add(pair, int.Parse(item.Element("symbol_id").Value));
                }
            }
            ConfigTool.TradePairDict = symbolDict;

            var notMatchList = new List<string>();
            foreach(var item in symbolDict)
            {
                var baseCoinArray2 = item.Key.Split('_');
                if (!coinDict.ContainsKey(baseCoinArray2[0])){
                    notMatchList.Add(baseCoinArray2[0]);
                }
            }

            ConfigTool.CurrentPair = symbolDict.ElementAt(0);

            var baseCoinArray = ConfigTool.CurrentPair.Key.Split('_');
            ConfigTool.CurrentBase = baseCoinArray[1];
            ConfigTool.CurrentCoin = baseCoinArray[0];

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DragonExMiningTool());
        }
    }
}
