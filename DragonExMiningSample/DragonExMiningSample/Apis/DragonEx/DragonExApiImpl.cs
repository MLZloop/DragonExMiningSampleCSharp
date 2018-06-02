using DragonExMiningSampleCSharp.Apis.Entity;
using DragonExMiningSampleCSharp.Apis.Log;
using DragonExMiningSampleCSharp.Apis.DragonEx;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static DragonExMiningSampleCSharp.Apis.Common.Enums;
using DragonExMiningSampleCSharp.Apis.Config;
using DragonExMiningSampleCSharp.Apis.Common;

namespace DragonExMiningSampleCSharp.Apis.DragonEx
{
    class DragonExApiImpl
    {
        /// <summary>
        /// Name
        /// </summary>
        private AccountSide name;

        /// <summary>
        /// Instance for private Apis
        /// </summary>
        private DragonExPrivateApis privateApiInstance = null;

        /// <summary>
        /// User Amounts
        /// </summary>
        public UserAmounts UserAmounts
        {
            get;
            set;
        }

        /// <summary>
        /// Trade depth entity
        /// </summary>
        public TradeDepthEntity Tde
        {
            get;
            set;
        }

        /// <summary>
        /// Constructor with parameter
        /// </summary>
        /// <param name="secretKey"></param>
        /// <param name="accessKey"></param>
        public DragonExApiImpl(string secretKey, string accessKey, AccountSide name)
        {
            this.name = name;
            privateApiInstance = new DragonExPrivateApis(secretKey, accessKey);
            UserAmounts = new UserAmounts(Constants.TRADE_NAME_DRAGON_EX, 0, 0);
        }

        /// <summary>
        /// Update user info
        /// </summary>
        public void UpdateUserInfo(bool pairChanged)
        {
            if (ConfigTool.RunMode == RunMode.TEST)
            {
                if (pairChanged)
                {
                    UserAmounts = new UserAmounts(Constants.TRADE_NAME_DRAGON_EX, 100, 1);
                }
            }
            else
            {
                UserAmounts = GetUserInfo();
            }
        }

        /// <summary>
        /// Get user informations
        /// </summary>
        /// <returns>User Amounts Entity</returns>
        public UserAmounts GetUserInfo()
        {
            UserAmounts currentUserAmounts = new UserAmounts(Constants.TRADE_NAME_DRAGON_EX, 0, 0.00m);
            var userInfo = privateApiInstance.GetUserOwn();
            var funds = userInfo.Elements("data");
            var baseId = ConfigTool.CoinDict[ConfigTool.CurrentBase].ToString();
            var coinId = ConfigTool.CoinDict[ConfigTool.CurrentCoin].ToString();
            foreach (var item in funds)
            {
                if (string.Equals(item.Element("coin_id").Value, baseId))
                {
                    currentUserAmounts.BaseAmount = (decimal)double.Parse(item.Element("volume").Value);
                }
                else if (string.Equals(item.Element("coin_id").Value, coinId))
                {
                    currentUserAmounts.CoinAmount = (decimal)double.Parse(item.Element("volume").Value);
                }
            }
            currentUserAmounts.UpdatedDate = DateTime.Now;
            return currentUserAmounts;
        }

        /// <summary>
        /// Check order succeed
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <returns>If succeed:true, else false.</returns>
        public bool CheckOrderSucceed(string orderId)
        {
            try
            {
                if(ConfigTool.RunMode == RunMode.TEST)
                {
                    if (TestConfigTool.CheckAOrderFailed && name == AccountSide.A)
                    {
                        return false;
                    }
                    else if (TestConfigTool.CheckBOrderFailed && name == AccountSide.B)
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    var data = privateApiInstance.GetOrderDetail(ConfigTool.CurrentPair.Value, orderId);
                    var receivedValue = data.Element("data").Element("status").Value;
                    //1:UNFILLED，2:FILLED,3:CANCELED
                    if (!string.IsNullOrEmpty(receivedValue)
                        && string.Equals(receivedValue.ToUpper(), "2"))
                    {
                        return true;
                    }
                    else
                    {
                        var logMsg = "Error happened when check dragonex trading succeed.The result is:" + data.ToString();
                        LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                    }
                }
            }
            catch (Exception ex)
            {
                var logMsg = "Exception happened when check dragonex trading succeed:" + Environment.NewLine
                    + ex.Message + Environment.NewLine + ex.StackTrace;
                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
            }

            return false;
        }

        /// <summary>
        /// Trade base to coin
        /// </summary>
        /// <param name="tradePair">Trade pair</param>
        /// <param name="price">Base price</param>
        /// <param name="amount">Trading amount</param>
        /// <returns>Trade entity</returns>
        public TradeEntity TradeBaseToCoin(int tradePair, decimal price, decimal amount)
        {
            return Trade(tradePair, TradeTypes.BUY, price, amount);
        }

        /// <summary>
        /// Trade coin to base
        /// </summary>
        /// <param name="tradePair">Trade pair</param>
        /// <param name="price">Base price</param>
        /// <param name="amount">Trading amount</param>
        /// <returns>Trade entity</returns>
        public TradeEntity TradeCoinToBase(int tradePair, decimal price, decimal amount)
        {
            return Trade(tradePair, TradeTypes.SELL, price, amount);
        }

        /// <summary>
        /// Cancel order
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public bool CancelOrder(string orderId)
        {
            try
            {
                if (ConfigTool.RunMode == RunMode.TEST)
                {
                    if (TestConfigTool.CancelAOrderFailed && name == AccountSide.A)
                    {
                        return false;
                    }
                    else if (TestConfigTool.CancelBOrderFailed && name == AccountSide.B)
                    {
                        return false;
                    }
                    return true;
                }
                XElement data = privateApiInstance.CancelOrder(ConfigTool.CurrentPair.Value, orderId);
                var id = data.Element("data").Element("order_id");
                if (id != null && !string.IsNullOrEmpty(id.Value))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                // If any error happened when Trading, out error log
                var logMsg = "Exception happened when cancel trading in dragonex.Exception:" + Environment.NewLine
                    + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine;
                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                return false;
            }
        }

        /// <summary>
        /// Trade
        /// </summary>
        /// <param name="symbolId">Symbol id</param>
        /// <param name="side">buy or sell</param>
        /// <param name="price">Base Price</param>
        /// <param name="amount">Amount</param>
        /// <param name="retryCount">Retry count when failed</param>
        /// <returns>Trade entity</returns>
        private TradeEntity Trade(int symbolId, TradeTypes side, decimal price, decimal amount, int retryCount = 1)
        {
            if (retryCount > ConfigTool.TradeFailedRetryCount)
            {
                // If retry ended, return
                return null;
            }
            TradeEntity te = new TradeEntity();
            bool isSucceeded = false;
            try
            {
                if(ConfigTool.RunMode == RunMode.TEST)
                {
                    if (side == TradeTypes.BUY && TestConfigTool.TradeBaseToCoinFailed)
                    {
                        return null;
                    }
                    else if (side == TradeTypes.SELL && TestConfigTool.TradeCoinToBaseFailed)
                    {
                        return null;
                    }
                    else
                    {
                        // If test
                        var receivedValue = amount.ToString();
                        var remainsValue = amount.ToString();
                        var orderId = "1";
                        var receivedD = (decimal)double.Parse(receivedValue);
                        var remainsD = (decimal)double.Parse(remainsValue);
                        te.Received = receivedD;
                        te.Remains = remainsD;
                        te.OrderId = orderId;
                        isSucceeded = true;
                    }
                }
                else
                {
                    XElement data = null;

                    if (side == TradeTypes.BUY)
                    {
                        //Try to buy
                        data = privateApiInstance.BuyOrder(symbolId, price.ToString(), amount.ToString());
                    }
                    else
                    {
                        //Try to sell
                        data = privateApiInstance.SellOrder(symbolId, price.ToString(), amount.ToString());
                    }

                    var id = data.Element("data").Element("order_id");
                    if (id != null && !string.IsNullOrEmpty(id.Value))
                    {
                        // If id is returned
                        var receivedValue = data.Element("data").Element("trade_volume").Value;
                        var remainsValue = data.Element("data").Element("volume").Value;
                        var orderId = id.Value;
                        var receivedD = (decimal)double.Parse(receivedValue);
                        var remainsD = (decimal)double.Parse(remainsValue);
                        te.Received = receivedD;
                        te.Remains = remainsD;
                        te.OrderId = orderId;
                        isSucceeded = true;
                    }
                    else
                    {
                        // If result is incorrect, output error log and retry if retry count is set
                        var logMsg = "Error happened when trading in dragonex.The result is:" + data.ToString();
                        LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                        return Trade(symbolId, side, price, amount, ++retryCount);
                    }
                }
            }
            catch (Exception ex)
            {
                // If any error happened when Trading, out error log
                var logMsg = "Exception happened when trading in dragonex.Exception:" + Environment.NewLine
                    + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine;
                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                return Trade(symbolId, side, price, amount, ++retryCount);
            }
            te.Success = isSucceeded;
            return te;
        }
        
        /// <summary>
        /// Update trade depth
        /// </summary>
        /// <param name="symbolId"></param>
        public void UpdateTradeDepth(int symbolId)
        {
            this.Tde = GetTradeDepth(symbolId);
        }

        /// <summary>
        /// Get trade Depth
        /// </summary>
        /// <returns></returns>
        public TradeDepthEntity GetTradeDepth(int symbolId)
        {
            try
            {
                var buyResult = DragonExPublicApis.Instance.GetMarketBuy(symbolId);
                var sellResult = DragonExPublicApis.Instance.GetMarketSell(symbolId);

                TradeDepthEntity tde = new TradeDepthEntity();
                tde.TradeName = Constants.TRADE_NAME_DRAGON_EX;
                tde.AsksList = new List<AskBidEntity>();
                tde.BidsList = new List<AskBidEntity>();
                var bidsNodes = buyResult.Elements("data");
                var asksNodes = sellResult.Elements("data");
                AskBidEntity tempItem;
                int getRow = Constants.DEPTH_DISPLAY_ROW - 1;
                if (asksNodes.Count() < Constants.DEPTH_DISPLAY_ROW)
                {
                    getRow = asksNodes.Count() - 1;
                }

                for (int i = getRow; i >= 0; i--)
                {
                    var asksItem = asksNodes.ElementAt(i);
                    tempItem = new AskBidEntity();
                    tempItem.No = i + 1;
                    tempItem.Price = decimal.Parse(CommonUtils.GetAskBidElementValue(asksItem.FirstNode));
                    tempItem.Amount = decimal.Parse(CommonUtils.GetAskBidElementValue(asksItem.LastNode));
                    tde.AsksList.Add(tempItem);
                }
                tde.MinAsk = tde.AsksList[getRow];
                tde.MinSecAsk = tde.AsksList[getRow - 1];
                tde.MinThdAsk = tde.AsksList[getRow - 2];

                getRow = Constants.DEPTH_DISPLAY_ROW - 1;
                if (bidsNodes.Count() < Constants.DEPTH_DISPLAY_ROW)
                {
                    getRow = bidsNodes.Count() - 1;
                }

                for (int i = 0; i <= getRow; i++)
                {
                    var bidsItem = bidsNodes.ElementAt(i);
                    tempItem = new AskBidEntity();
                    tempItem.No = i + 1;
                    tempItem.Price = decimal.Parse(CommonUtils.GetAskBidElementValue(bidsItem.FirstNode));
                    tempItem.Amount = decimal.Parse(CommonUtils.GetAskBidElementValue(bidsItem.LastNode));
                    tde.BidsList.Add(tempItem);
                }
                tde.MaxBid = tde.BidsList[0];
                tde.MaxSecBid = tde.BidsList[1];
                tde.MaxThdBid = tde.BidsList[2];
                return tde;
            }
            catch (Exception ex)
            {
                // If any error happened when Trading, out error log
                var logMsg = "Exception happened when getting market info in dragonex.Exception:" + Environment.NewLine
                    + ex.Message + Environment.NewLine + ex.StackTrace + Environment.NewLine;
                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
            }
            return null;
        }
    }
}
