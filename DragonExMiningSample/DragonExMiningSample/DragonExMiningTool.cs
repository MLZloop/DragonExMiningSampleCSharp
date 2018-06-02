using DragonExMiningSampleCSharp.Apis.Common;
using DragonExMiningSampleCSharp.Apis.Config;
using DragonExMiningSampleCSharp.Apis.DragonEx;
using DragonExMiningSampleCSharp.Apis.Entity;
using DragonExMiningSampleCSharp.Apis.Log;
using DragonExMiningSampleCSharp.MultiLanguage;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DragonExMiningSampleCSharp.Apis.Common.Enums;

namespace DragonExMiningSampleCSharp
{
    public partial class DragonExMiningTool : Form
    {
        /// <summary>
        /// Randome tool
        /// </summary>
        private Random randomTool = new Random(int.Parse(DateTime.Now.ToString("yyyyMMddHHmm").Substring(2)));

        /// <summary>
        /// Api For A Account
        /// </summary>
        private DragonExApiImpl dragonExApiForA = new DragonExApiImpl(DragonExConstants.SECRET_KEY_A, DragonExConstants.ACCESS_KEY_A, AccountSide.A);

        /// <summary>
        /// Api For B Account
        /// </summary>
        private DragonExApiImpl dragonExApiForB = new DragonExApiImpl(DragonExConstants.SECRET_KEY_B, DragonExConstants.ACCESS_KEY_B, AccountSide.B);

        /// <summary>
        /// Thread for get market info
        /// </summary>
        private Thread getMarketInfoThread;

        /// <summary>
        /// Thread for get user info
        /// </summary>
        private Thread getUserInfoThread;

        /// <summary>
        /// Trading thread
        /// </summary>
        private Thread tradingThread;

        /// <summary>
        /// Trading entity
        /// </summary>
        private TradingEntity tradingEntity;

        /// <summary>
        /// Whether auto trading
        /// </summary>
        private bool autoTrading = false;

        /// <summary>
        /// Whether auto generate mine price
        /// </summary>
        private bool autoGenerateMinePrice = false;

        /// <summary>
        /// Mine price
        /// </summary>
        private decimal minePrice = 0.0000000001m;

        /// <summary>
        /// Previous mine price
        /// </summary>
        private decimal previousMinePrice = 0.0000000001m;

        /// <summary>
        /// Whether mine amount is unlimited
        /// </summary>
        private bool mineAmountUnlimited = false;

        /// <summary>
        /// Mine amount
        /// </summary>
        private decimal mineAmount = 0.0000000001m;

        /// <summary>
        /// Previous mine amount
        /// </summary>
        private decimal previousMineAmount = 0.0000000001m;

        /// <summary>
        /// Wheter trade interval is unlimited
        /// </summary>
        private bool tradeIntervalUnlimited = false;

        /// <summary>
        /// Trade interval
        /// </summary>
        private int tradeInterval = 10;

        /// <summary>
        /// Trade side
        /// </summary>
        private TradeSide tradeSide = TradeSide.BOTH_A_B_A_FIRST;

        /// <summary>
        /// Trade method
        /// </summary>
        private TradeMethod tradeMethod = TradeMethod.A_TO_B;

        /// <summary>
        /// Previous trade method
        /// </summary>
        private TradeMethod previousTradeMethod = TradeMethod.A_TO_B;

        /// <summary>
        /// Max Mine Amount
        /// </summary>
        private decimal maxMineAmount = 1;

        /// <summary>
        /// Previous max mine amount
        /// </summary>
        private decimal previousMaxMineAmount = 1;

        /// <summary>
        /// Whether max mine amount is unlimited
        /// </summary>
        private bool maxMineAmountUnlimited = true;

        /// <summary>
        /// Current side mine amount total
        /// </summary>
        private decimal currentSideMineAmountTotal = 0.0m;

        /// <summary>
        /// Whether random mine amount
        /// </summary>
        private bool isRandomMineAmount = false;

        /// <summary>
        /// Random mine amount to
        /// </summary>
        private decimal randomMineAmounTo = 1;

        /// <summary>
        /// Previous Random mine amount to
        /// </summary>
        private decimal previousRndomMineAmounTo = 1;

        /// <summary>
        /// Whether random mine amount to unlimited
        /// </summary>
        private bool randomMineAmountToUnlimited = false;

        /// <summary>
        /// Last trade time
        /// </summary>
        private long lastTradeTime = 0;

        /// <summary>
        /// Whether pair is changed
        /// </summary>
        private bool pairChanged = true;
        
        /// <summary>
        /// Whether is trading
        /// </summary>
        private bool isTrading = false;

        /// <summary>
        /// Reset trade depth
        /// </summary>
        /// <param name="tde"></param>
        private delegate void ResetTradeDepth(TradeDepthEntity tde);

        /// <summary>
        /// Reset UI
        /// </summary>
        private delegate void ResetUI();

        /// <summary>
        /// Profits entity
        /// </summary>
        private ProfitsEntity profits = new ProfitsEntity();

        public DragonExMiningTool()
        {
            InitializeComponent();
            MultiLanConfig.Instance.ChangeMultiLan(this);
        }

        /// <summary>
        /// Execute get user info
        /// </summary>
        public void ExecGetUserInfos()
        {
            while (true)
            {
                if (!isTrading)
                {
                    // Only execute when not trading
                    GetUserInfo();
                }
                Thread.Sleep(ConfigTool.UserInfoGetInterval);
            }
        }

        /// <summary>
        /// Get user info
        /// </summary>
        public void GetUserInfo()
        {
            try
            {
                dragonExApiForA.UpdateUserInfo(pairChanged);
                dragonExApiForB.UpdateUserInfo(pairChanged);
                pairChanged = false;
                this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });
            }
            catch (ThreadAbortException)
            {
                //Do nothing when aborting thread
            }
            catch (Exception ex)
            {
                //Error happened when getting
                var logMsg = MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_GETTING_USER_INFO") 
                    + Environment.NewLine
                    + ex.Message 
                    + Environment.NewLine 
                    + ex.StackTrace;
                Console.WriteLine(logMsg);
                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
            }
        }

        /// <summary>
        /// Reset current user infos
        /// </summary>
        private void ResetCurrentUserInfos()
        {
            if (dragonExApiForA.UserAmounts != null)
            {
                aBaseLbl.Text = dragonExApiForA.UserAmounts.BaseAmount.ToString();
                aCoinLbl.Text = dragonExApiForA.UserAmounts.CoinAmount.ToString();
                aUpdatedLbl.Text = dragonExApiForA.UserAmounts.UpdatedDate.ToString("yyyy/MM/dd HH:mm:ss");
            }

            if (dragonExApiForB.UserAmounts != null)
            {
                bBaseLbl.Text = dragonExApiForB.UserAmounts.BaseAmount.ToString();
                bCoinLbl.Text = dragonExApiForB.UserAmounts.CoinAmount.ToString();
                bUpdatedLbl.Text = dragonExApiForB.UserAmounts.UpdatedDate.ToString("yyyy/MM/dd HH:mm:ss");
            }

            if (dragonExApiForA.UserAmounts != null
                && dragonExApiForA.UserAmounts != null)
            {
                profits.UpdateCurrent(dragonExApiForA.UserAmounts, dragonExApiForB.UserAmounts);
                baseBaseLbl.Text = profits.BaseBaseAmount.ToString();
                baseCoinLbl.Text = profits.BaseCoinAmount.ToString();
                currentBaseLbl.Text = profits.CurrentBaseAmount.ToString();
                currentCoinLbl.Text = profits.CurrentCoinAmount.ToString();
                profitsBaseLbl.Text = profits.ProfitsBaseAmount.ToString();
                profitsCoinLbl.Text = profits.ProfitsCoinAmount.ToString();
            }

            totalLostLbl.Text = currentTradeLostCoinAmount.ToString();
            totalPlusLbl.Text = currentTradePlusCoinAmount.ToString();
        }

        /// <summary>
        /// Execute get market info
        /// </summary>
        public void ExecGetMarketInfo()
        {
            while (true)
            {
                try
                {
                    dragonExApiForA.UpdateTradeDepth(ConfigTool.CurrentPair.Value);
                    this.Invoke(new ResetTradeDepth(ResetCurrentDepth), new object[] { dragonExApiForA.Tde });
                    this.Invoke(new ResetUI(UpdateCurrentUI), new object[] { });
                }
                catch (ThreadAbortException)
                {
                    //Do nothing when aborting thread
                }
                catch (Exception ex)
                {
                    //Error happened when getting
                    var logMsg = MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_GETTING_TICKER") 
                        + Environment.NewLine
                        + ex.Message 
                        + Environment.NewLine + ex.StackTrace;
                    Console.WriteLine(logMsg);
                    LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Reset current depth
        /// </summary>
        /// <param name="tde">Trade depth entity</param>
        private void ResetCurrentDepth(TradeDepthEntity tde)
        {
            if (tde == null)
            {
                return;
            }
            AskGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            AskGrid.ReadOnly = true;
            BidGrid.SelectionMode = DataGridViewSelectionMode.CellSelect;
            BidGrid.ReadOnly = true;
            AskGrid.DataSource = tde.AsksList;
            AskGrid.AutoResizeColumns();
            BidGrid.DataSource = tde.BidsList;
            BidGrid.AutoResizeColumns();
        }

        /// <summary>
        /// Update Current UI
        /// </summary>
        private void UpdateCurrentUI()
        {
            if (dragonExApiForA.Tde == null)
            {
                return;
            }
            // If auto generate mine price
            if (autoGenerateMinePrice)
            {
                minePrice = (dragonExApiForA.Tde.MaxBid.Price + dragonExApiForA.Tde.MinAsk.Price) / 2;
                minePriceNud.Value = minePrice;
            }
            if (autoTrading)
            {
                ExecTrading();
            }
        }

        /// <summary>
        /// Execute trading
        /// </summary>
        private void ExecTrading()
        {
            if (!lostCoinLimitUnlimited)
            {
                if (currentTradeLostCoinAmount >= lostCoinLimit)
                {
                    logTxt.Text = "";
                    ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_ENDED_REACHED_LOST_LIMIT"), true);
                    return;
                }
                if (currentTradePlusCoinAmount >= lostCoinLimit)
                {
                    logTxt.Text = "";
                    ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_ENDED_REACHED_PLUS_LIMIT"), true);
                    return;
                }
            }

            if (previousTradeMethod != tradeMethod)
            {
                // If trade method changed.Reset current side total traded amount.
                currentSideMineAmountTotal = 0.0m;
            }
            previousTradeMethod = tradeMethod;

            switch (tradeSide)
            {
                case TradeSide.A_TO_B:
                    tradeMethod = TradeMethod.A_TO_B;
                    // If not trading now
                    if (tradingThread == null || !tradingThread.IsAlive)
                    {
                        decimal currentMineAmount = mineAmount;
                        // If mine amount is unlimited
                        if (mineAmountUnlimited)
                        {
                            var aMaxAmount = dragonExApiForA.UserAmounts.CoinAmount;
                            var bMaxAmount = dragonExApiForB.UserAmounts.BaseAmount / (minePrice * (1 + ConfigTool.TradeFee));
                            currentMineAmount = Math.Min(aMaxAmount, bMaxAmount);
                            if (!maxMineAmountUnlimited)
                            {
                                currentMineAmount = Math.Min(currentMineAmount, maxMineAmount);
                            }
                        }
                        else if (isRandomMineAmount)
                        {
                            decimal minAmount = currentMineAmount;
                            // If amount is random
                            if (randomMineAmountToUnlimited)
                            {
                                decimal maxAmount = 0.0m;
                                if (maxMineAmountUnlimited)
                                {
                                    var aMaxAmount = dragonExApiForA.UserAmounts.CoinAmount;
                                    var bMaxAmount = dragonExApiForB.UserAmounts.BaseAmount / (minePrice * (1 + ConfigTool.TradeFee));
                                    maxAmount = Math.Min(aMaxAmount, bMaxAmount);
                                }
                                else
                                {
                                    maxAmount = maxMineAmount - currentSideMineAmountTotal;
                                }
                                if (maxAmount > minAmount)
                                {
                                    decimal rdm = (decimal)randomTool.NextDouble();
                                    currentMineAmount = CommonUtils.GetTruncateDecimal(minAmount + (maxAmount - currentMineAmount) * rdm, ConfigTool.Digits);
                                }
                                else
                                {
                                    currentMineAmount = minAmount;
                                }
                            }
                            else
                            {
                                decimal maxAmount = randomMineAmounTo;
                                decimal rdm = (decimal)randomTool.NextDouble();
                                currentMineAmount = CommonUtils.GetTruncateDecimal(minAmount + (maxAmount - currentMineAmount) * rdm, ConfigTool.Digits);
                            }
                        }
                        if (currentMineAmount * minePrice > ConfigTool.MinimumTradeUsdtAmount)
                        {
                            if (mineAmountUnlimited ||
                                (currentMineAmount <= dragonExApiForA.UserAmounts.CoinAmount
                                && currentMineAmount * minePrice * (1 + ConfigTool.TradeFee) <= dragonExApiForB.UserAmounts.BaseAmount))
                            {
                                if (maxMineAmountUnlimited ||
                                    currentSideMineAmountTotal + currentMineAmount <= maxMineAmount)
                                {
                                    // Do trading
                                    long currentTS = DateTime.Now.Ticks;
                                    if (tradeIntervalUnlimited || currentTS - lastTradeTime >= tradeInterval * 10000000)
                                    {
                                        lastTradeTime = currentTS;
                                        logTxt.Clear();
                                        tradingEntity = new TradingEntity();
                                        tradingEntity.Buy = minePrice;
                                        tradingEntity.Sell = minePrice;
                                        tradingEntity.Amount = currentMineAmount;
                                        tradingThread = new Thread(ExecTradingAB);
                                        tradingThread.Start();
                                    }
                                }
                                else
                                {
                                    logTxt.Text = "";
                                    ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_REACH_MAX_AMOUNT"), true);
                                }
                            }
                            else
                            {
                                if (!mineAmountUnlimited)
                                {
                                    logTxt.Text = "";
                                    ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_ENDED"), true);
                                }
                            }
                        }
                        else
                        {
                            logTxt.Text = "";
                            ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_ENDED"), true);
                        }
                    }
                    break;
                case TradeSide.B_TO_A:
                    tradeMethod = TradeMethod.B_TO_A;
                    // If not trading now
                    if (tradingThread == null || !tradingThread.IsAlive)
                    {
                        decimal currentMineAmount = mineAmount;
                        // If mine amount is unlimited
                        if (mineAmountUnlimited)
                        {
                            var aMaxAmount = dragonExApiForB.UserAmounts.CoinAmount;
                            var bMaxAmount = dragonExApiForA.UserAmounts.BaseAmount / (minePrice * (1 + ConfigTool.TradeFee));
                            currentMineAmount = Math.Min(aMaxAmount, bMaxAmount);
                            if (!maxMineAmountUnlimited)
                            {
                                currentMineAmount = Math.Min(currentMineAmount, maxMineAmount);
                            }
                        }
                        else if (isRandomMineAmount)
                        {
                            decimal minAmount = currentMineAmount;
                            // If amount is random
                            if (randomMineAmountToUnlimited)
                            {
                                decimal maxAmount = 0.0m;
                                if (maxMineAmountUnlimited)
                                {
                                    var aMaxAmount = dragonExApiForB.UserAmounts.CoinAmount;
                                    var bMaxAmount = dragonExApiForA.UserAmounts.BaseAmount / (minePrice * (1 + ConfigTool.TradeFee));
                                    maxAmount = Math.Min(aMaxAmount, bMaxAmount);
                                }
                                else
                                {
                                    maxAmount = maxMineAmount - currentSideMineAmountTotal;
                                }
                                if (maxAmount > minAmount)
                                {
                                    decimal rdm = (decimal)randomTool.NextDouble();
                                    currentMineAmount = CommonUtils.GetTruncateDecimal(minAmount + (maxAmount - currentMineAmount) * rdm, ConfigTool.Digits);
                                }
                                else
                                {
                                    currentMineAmount = minAmount;
                                }
                            }
                            else
                            {
                                decimal maxAmount = randomMineAmounTo;
                                decimal rdm = (decimal)randomTool.NextDouble();
                                currentMineAmount = CommonUtils.GetTruncateDecimal(minAmount + (maxAmount - currentMineAmount) * rdm, ConfigTool.Digits);
                            }
                        }
                        if (currentMineAmount * minePrice > ConfigTool.MinimumTradeUsdtAmount)
                        {
                            if (mineAmountUnlimited ||
                                (currentMineAmount <= dragonExApiForB.UserAmounts.CoinAmount
                                && currentMineAmount * minePrice * (1 + ConfigTool.TradeFee) <= dragonExApiForA.UserAmounts.BaseAmount))
                            {
                                if (maxMineAmountUnlimited ||
                                    currentSideMineAmountTotal + currentMineAmount <= maxMineAmount)
                                {
                                    // Do trading
                                    long currentTS = DateTime.Now.Ticks;
                                    if (tradeIntervalUnlimited || currentTS - lastTradeTime >= tradeInterval * 10000000)
                                    {
                                        lastTradeTime = currentTS;
                                        logTxt.Clear();
                                        tradingEntity = new TradingEntity();
                                        tradingEntity.Buy = minePrice;
                                        tradingEntity.Sell = minePrice;
                                        tradingEntity.Amount = currentMineAmount;
                                        tradingThread = new Thread(ExecTradingBA);
                                        tradingThread.Start();
                                    }
                                }
                                else
                                {
                                    logTxt.Text = "";
                                    ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_REACH_MAX_AMOUNT"), true);
                                }
                            }
                            else
                            {
                                if (!mineAmountUnlimited)
                                {
                                    logTxt.Text = "";
                                    ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_ENDED"), true);
                                }
                            }
                        }
                        else
                        {
                            logTxt.Text = "";
                            ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_ENDED"), true);
                        }
                    }
                    break;
                case TradeSide.BOTH_A_B_A_FIRST:
                case TradeSide.BOTH_A_B_B_FIRST:
                    // If not trading now
                    if (tradingThread == null || !tradingThread.IsAlive)
                    {
                        decimal currentMineAmount = mineAmount;
                        // If mine amount is unlimited
                        if (mineAmountUnlimited)
                        {
                            var aMaxAmount = dragonExApiForA.UserAmounts.CoinAmount;
                            var bMaxAmount = dragonExApiForB.UserAmounts.BaseAmount / (minePrice * (1 + ConfigTool.TradeFee));
                            var aToBMineAmount = Math.Min(aMaxAmount, bMaxAmount);
                            if (!maxMineAmountUnlimited)
                            {
                                aToBMineAmount = Math.Min(aToBMineAmount, maxMineAmount);
                            }

                            aMaxAmount = dragonExApiForB.UserAmounts.CoinAmount;
                            bMaxAmount = dragonExApiForA.UserAmounts.BaseAmount / (minePrice * (1 + ConfigTool.TradeFee));
                            var bToAMineAmount = Math.Min(aMaxAmount, bMaxAmount);
                            if (!maxMineAmountUnlimited)
                            {
                                bToAMineAmount = Math.Min(bToAMineAmount, maxMineAmount);
                            }

                            if (aToBMineAmount * minePrice <= ConfigTool.MinimumTradeUsdtAmount
                                && bToAMineAmount * minePrice <= ConfigTool.MinimumTradeUsdtAmount)
                            {
                                logTxt.Text = "";
                                ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_UNDER_MINIMUM"), true);
                                return;
                            }

                            switch (tradeMethod)
                            {
                                case TradeMethod.A_TO_B:
                                    currentMineAmount = aToBMineAmount;
                                    break;
                                case TradeMethod.B_TO_A:
                                    currentMineAmount = bToAMineAmount;
                                    break;
                            }
                        }
                        else if (isRandomMineAmount)
                        {
                            decimal minAmount = currentMineAmount;
                            // If amount is random
                            if (randomMineAmountToUnlimited)
                            {
                                decimal maxAmount = 0.0m;
                                if (maxMineAmountUnlimited)
                                {
                                    var aMaxAmount = dragonExApiForA.UserAmounts.CoinAmount;
                                    var bMaxAmount = dragonExApiForB.UserAmounts.BaseAmount / (minePrice * (1 + ConfigTool.TradeFee));
                                    var aToBMineAmount = Math.Min(aMaxAmount, bMaxAmount);

                                    aMaxAmount = dragonExApiForB.UserAmounts.CoinAmount;
                                    bMaxAmount = dragonExApiForA.UserAmounts.BaseAmount / (minePrice * (1 + ConfigTool.TradeFee));
                                    var bToAMineAmount = Math.Min(aMaxAmount, bMaxAmount);

                                    switch (tradeMethod)
                                    {
                                        case TradeMethod.A_TO_B:
                                            maxAmount = aToBMineAmount;
                                            break;
                                        case TradeMethod.B_TO_A:
                                            maxAmount = bToAMineAmount;
                                            break;
                                    }
                                }
                                else
                                {
                                    maxAmount = maxMineAmount - currentSideMineAmountTotal;
                                }
                                if (maxAmount > minAmount)
                                {
                                    decimal rdm = (decimal)randomTool.NextDouble();
                                    currentMineAmount = CommonUtils.GetTruncateDecimal(minAmount + (maxAmount - currentMineAmount) * rdm, ConfigTool.Digits);
                                }
                                else
                                {
                                    currentMineAmount = minAmount;
                                }
                            }
                            else
                            {
                                decimal maxAmount = randomMineAmounTo;
                                decimal rdm = (decimal)randomTool.NextDouble();
                                currentMineAmount = CommonUtils.GetTruncateDecimal(minAmount + (maxAmount - currentMineAmount) * rdm, ConfigTool.Digits);
                            }
                        }
                        if (currentMineAmount * minePrice > ConfigTool.MinimumTradeUsdtAmount)
                        {
                            if (mineAmountUnlimited ||
                                (tradeMethod == TradeMethod.A_TO_B && currentMineAmount <= dragonExApiForA.UserAmounts.CoinAmount
                                && currentMineAmount * minePrice * (1 + ConfigTool.TradeFee) <= dragonExApiForB.UserAmounts.BaseAmount) ||
                                (tradeMethod == TradeMethod.B_TO_A && currentMineAmount <= dragonExApiForB.UserAmounts.CoinAmount
                                && currentMineAmount * minePrice * (1 + ConfigTool.TradeFee) <= dragonExApiForA.UserAmounts.BaseAmount))
                            {
                                if (maxMineAmountUnlimited ||
                                    currentSideMineAmountTotal + currentMineAmount <= maxMineAmount)
                                {
                                    // Do trading
                                    long currentTS = DateTime.Now.Ticks;
                                    if (tradeIntervalUnlimited || currentTS - lastTradeTime >= tradeInterval * 10000000)
                                    {
                                        lastTradeTime = currentTS;
                                        logTxt.Clear();
                                        lastTradeTime = currentTS;
                                        tradingEntity = new TradingEntity();
                                        tradingEntity.Buy = minePrice;
                                        tradingEntity.Sell = minePrice;
                                        tradingEntity.Amount = currentMineAmount;
                                        switch (tradeMethod)
                                        {
                                            case TradeMethod.A_TO_B:
                                                tradingThread = new Thread(ExecTradingAB);
                                                break;
                                            case TradeMethod.B_TO_A:
                                                tradingThread = new Thread(ExecTradingBA);
                                                break;
                                        }
                                        tradingThread.Start();
                                    }
                                }
                                else
                                {
                                    // If mine amount is reached max amount, then go to another side
                                    logTxt.Text = "";
                                    ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_REACH_MAX_LIMIT_REVERT_SIDE"), true);
                                    RevertTradeMethod();
                                }
                            }
                            else
                            {
                                if (!mineAmountUnlimited)
                                {
                                    // If mine amount is not unlimited, then go to another side
                                    logTxt.Text = "";
                                    ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_UNDER_MINIMUM_REVERT_SIDE"), true);
                                    RevertTradeMethod();
                                }
                            }
                        }
                        else
                        {
                            if (mineAmountUnlimited)
                            {
                                // If mine amount is unlimited, then go to another side
                                logTxt.Text = "";
                                ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_UNDER_MINIMUM_REVERT_SIDE"), true);
                                RevertTradeMethod();
                            }
                            else
                            {
                                logTxt.Text = "";
                                ResetTradingLog(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_UNDER_MINIMUM"), true);
                            }
                        }
                    }
                    break;
            }
        }

        private void RevertTradeMethod()
        {
            switch (tradeMethod)
            {
                case TradeMethod.A_TO_B:
                    tradeMethod = TradeMethod.B_TO_A;
                    currentSideMineAmountTotal = 0.0m;
                    break;
                case TradeMethod.B_TO_A:
                    tradeMethod = TradeMethod.A_TO_B;
                    currentSideMineAmountTotal = 0.0m;
                    break;
            }
        }

        private void DragonExMiningTool_Load(object sender, EventArgs e)
        {
            if (ConfigTool.RunMode == RunMode.NORMAL)
            {
                this.Width = 608;
            }

            var pairList = new List<string>();
            foreach(var item in ConfigTool.TradePairDict)
            {
                pairList.Add(item.Key);
            }
            pairCmb.DataSource = pairList;
            aAccBaseLbl.Text = ConfigTool.CurrentBase.ToUpper() + ":";
            aAccCoinLbl.Text = ConfigTool.CurrentCoin.ToUpper() + ":";
            bAccBaseLbl.Text = ConfigTool.CurrentBase.ToUpper() + ":";
            bAccCoinLbl.Text = ConfigTool.CurrentCoin.ToUpper() + ":";
            profitsAllBaseLbl.Text = MultiLanConfig.Instance.GetKeyValue("LABEL_ALL")  + " " + ConfigTool.CurrentBase.ToUpper() + ":";
            profitsAllCoinLbl.Text = MultiLanConfig.Instance.GetKeyValue("LABEL_ALL")  + " " + ConfigTool.CurrentCoin.ToUpper() + ":";
            autoTradeChk.Checked = false;

            if (getMarketInfoThread == null || !getMarketInfoThread.IsAlive)
            {
                getMarketInfoThread = new Thread(ExecGetMarketInfo);
            }
            else
            {
                if (getMarketInfoThread.IsAlive)
                {
                    try
                    {
                        getMarketInfoThread.Abort();
                    }
                    catch (Exception)
                    {
                        //Abort exception, Ignore
                    }
                }
                getMarketInfoThread = new Thread(ExecGetMarketInfo);
            }
            getMarketInfoThread.Start();
            
            if (getUserInfoThread == null || !getUserInfoThread.IsAlive)
            {
                getUserInfoThread = new Thread(ExecGetUserInfos);
            }
            else
            {
                if (getUserInfoThread.IsAlive)
                {
                    try
                    {
                        getUserInfoThread.Abort();
                    }
                    catch (Exception)
                    {
                        //Abort exception, Ignore
                    }
                }
                getUserInfoThread = new Thread(ExecGetUserInfos);
            }
            getUserInfoThread = new Thread(ExecGetUserInfos);
            getUserInfoThread.Start();
        }

        private void DragonExMiningTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (getMarketInfoThread != null && getMarketInfoThread.IsAlive)
            {
                try
                {
                    getMarketInfoThread.Abort();
                }
                catch (Exception)
                {
                    //Abort exception, Ignore
                }
            }
            if (getUserInfoThread != null && getUserInfoThread.IsAlive)
            {
                try
                {
                    getUserInfoThread.Abort();
                }
                catch (Exception)
                {
                    //Abort exception, Ignore
                }
            }
            if (tradingThread != null && tradingThread.IsAlive)
            {
                try
                {
                    // If is trading, abort
                    tradingThread.Abort();
                }
                catch (Exception)
                {
                    //Abort exception, Ignore
                }
            }
        }

        private void pairCmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            string pair = pairCmb.SelectedValue.ToString();
            if (!string.IsNullOrEmpty(pair) 
                && ConfigTool.TradePairDict.ContainsKey(pair)
                && !string.Equals(ConfigTool.CurrentPair.Key, pair))
            {
                ConfigTool.CurrentPair = new KeyValuePair<string, int>(pair, ConfigTool.TradePairDict[pair]);

                var baseCoinArray = ConfigTool.CurrentPair.Key.Split('_');
                ConfigTool.CurrentBase = baseCoinArray[1];
                ConfigTool.CurrentCoin = baseCoinArray[0];

                aAccBaseLbl.Text = ConfigTool.CurrentBase.ToUpper() + ":";
                aAccCoinLbl.Text = ConfigTool.CurrentCoin.ToUpper() + ":";
                bAccBaseLbl.Text = ConfigTool.CurrentBase.ToUpper() + ":";
                bAccCoinLbl.Text = ConfigTool.CurrentCoin.ToUpper() + ":";
                profitsAllBaseLbl.Text = MultiLanConfig.Instance.GetKeyValue("LABEL_ALL")  + " " + ConfigTool.CurrentBase.ToUpper() + ":";
                profitsAllCoinLbl.Text = MultiLanConfig.Instance.GetKeyValue("LABEL_ALL")  + " " + ConfigTool.CurrentCoin.ToUpper() + ":";

                pairChanged = true;
                if (getUserInfoThread != null && getUserInfoThread.IsAlive)
                {
                    try
                    {
                        getUserInfoThread.Abort();
                    }
                    catch (Exception)
                    {
                        //Abort exception, Ignore
                    }
                }
                getUserInfoThread = new Thread(ExecGetUserInfos);
                getUserInfoThread.Start();
            }
        }

        private void setBaseBtn_Click(object sender, EventArgs e)
        {
            profits.UpdateBase(dragonExApiForA.UserAmounts, dragonExApiForB.UserAmounts);
            ResetCurrentUserInfos();
            currentTradeLostCoinAmount = 0.0m;
            currentTradePlusCoinAmount = 0.0m;
        }

        private void updateUserInfoBtn_Click(object sender, EventArgs e)
        {
            GetUserInfo();
        }

        private void autoTradeChk_CheckedChanged(object sender, EventArgs e)
        {
            bool isSet = true;
            if (autoTradeChk.Checked)
            {
                //Check if params is correct
                if (!autoGenerateMinePrice)
                {
                    //When not auto generate, need to check the ptice is between current buy and sell price
                    if(dragonExApiForA.Tde.MaxBid.Price >= minePrice 
                        || dragonExApiForA.Tde.MinAsk.Price <= minePrice)
                    {
                        isSet = false;
                        autoTradeChk.Checked = false;
                        MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MINE_PRICE_INCORRECT"),
                            MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                }
                if (!mineAmountUnlimited)
                {
                    //When mine amount is not unlimited, need to check mine amount is over minimum amount
                    //To ensure trading successfully, check all sell and buy price and amount is over
                    if(dragonExApiForA.Tde.MaxBid.Price * mineAmount <= ConfigTool.MinimumTradeUsdtAmount
                        || dragonExApiForA.Tde.MinAsk.Price * mineAmount <= ConfigTool.MinimumTradeUsdtAmount)
                    {
                        isSet = false;
                        autoTradeChk.Checked = false;
                        MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MINE_AMOUNT_INCORRECT")
                            + Environment.NewLine + MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MINE_AMOUNT_ENSURE"),
                            MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS") , MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                }
                if (isRandomMineAmount && !randomMineAmountToUnlimited)
                {
                    //When mine amount is random and mine amount to is unlimited, need to check mine amount to is over mine amount
                    if (randomMineAmounTo < mineAmount)
                    {
                        isSet = false;
                        autoTradeChk.Checked = false;
                        MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_RANDOM_MINE_AMOUNT_TO_UNDER_FROM"),
                            MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                }
                if (!maxMineAmountUnlimited)
                {
                    //When max mine amount is not unlimited, need to check max mine amount is over minimum amount and is over mine amount
                    //To ensure trading successfully, check all sell and buy price and amount is over
                    if (mineAmountUnlimited)
                    {
                        if (dragonExApiForA.Tde.MaxBid.Price * maxMineAmount <= ConfigTool.MinimumTradeUsdtAmount
                           || dragonExApiForA.Tde.MinAsk.Price * maxMineAmount <= ConfigTool.MinimumTradeUsdtAmount)
                        {
                            isSet = false;
                            autoTradeChk.Checked = false;
                            MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MAX_MINE_AMOUNT_UNDER_MINIMUM")
                                + Environment.NewLine + MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MINE_AMOUNT_ENSURE"),
                                MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }
                    }
                    else
                    {
                        if (maxMineAmount < mineAmount)
                        {
                            isSet = false;
                            autoTradeChk.Checked = false;
                            MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MAX_MINE_AMOUNT_UNDER_MINE_AMOUNT")
                                + Environment.NewLine + MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MINE_AMOUNT_ENSURE"),
                                MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }
                        if (isRandomMineAmount && !randomMineAmountToUnlimited)
                        {
                            if (randomMineAmounTo > maxMineAmount)
                            {
                                isSet = false;
                                autoTradeChk.Checked = false;
                                MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_RANDOM_MINE_AMOUNT_TO_UNDER_FROM"),
                                    MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                return;
                            }
                        }
                    }
                }
                if (!tradeIntervalUnlimited)
                {
                    //When trade interval is not unlimited, need to check whether trade interval is correct
                    if(tradeInterval <= 0)
                    {
                        isSet = false;
                        autoTradeChk.Checked = false;
                        MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_TRADE_INTERVAL"),
                            MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS") , MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                }
            }
            if (isSet)
            {
                autoTrading = autoTradeChk.Checked;
                tradeBtn.Enabled = !autoTrading;
                pairCmb.Enabled = !autoTrading;
                autoGenerateChk.Enabled = !autoTrading;
                mineAmountUnlimitedChk.Enabled = !autoTrading && !randomMineAmountChk.Checked;
                tradeIntervalUnlimitedChk.Enabled = !autoTrading;
                minePriceNud.Enabled = !autoTrading && !autoGenerateChk.Checked;
                mineAmountNud.Enabled = !autoTrading && !mineAmountUnlimitedChk.Checked;
                tradeIntervalNud.Enabled = !autoTrading && !tradeIntervalUnlimitedChk.Checked;
                randomMineAmountChk.Enabled = !autoTrading;
                mineAmountToNud.Enabled = !autoTrading && !mineAmountToUnlimitedChk.Checked;
                mineAmountToUnlimitedChk.Enabled = !autoTrading;
            }
        }

        private void autoGenerateChk_CheckedChanged(object sender, EventArgs e)
        {
            autoGenerateMinePrice = autoGenerateChk.Checked;
            if (!autoGenerateMinePrice)
            {
                minePriceNud.Value = previousMinePrice;
            }
            minePriceNud.Enabled = !autoGenerateMinePrice;
        }

        private void mineAmountUnlimitedChk_CheckedChanged(object sender, EventArgs e)
        {
            mineAmountUnlimited = mineAmountUnlimitedChk.Checked;
            if (!mineAmountUnlimited)
            {
                mineAmountNud.Value = previousMineAmount;
            }
            mineAmountNud.Enabled = !mineAmountUnlimited;
        }

        private void tradeIntervalUnlimitedChk_CheckedChanged(object sender, EventArgs e)
        {
            tradeIntervalUnlimited = tradeIntervalUnlimitedChk.Checked;
            tradeIntervalNud.Enabled = !tradeIntervalUnlimited;
        }

        private void minePriceNud_ValueChanged(object sender, EventArgs e)
        {
            if (!autoGenerateMinePrice)
            {
                minePrice = minePriceNud.Value;
                previousMinePrice = minePrice;
            }
        }

        private void mineAmountNud_ValueChanged(object sender, EventArgs e)
        {
            if (!mineAmountUnlimited)
            {
                mineAmount = mineAmountNud.Value;
                previousMineAmount = mineAmount;
            }
        }

        private void tradeIntervalNud_ValueChanged(object sender, EventArgs e)
        {
            if (!tradeIntervalUnlimited)
            {
                tradeInterval = (int)tradeIntervalNud.Value;
            }
        }

        private void tradeBtn_Click(object sender, EventArgs e)
        {
            if(tradeSide != TradeSide.A_TO_B && tradeSide != TradeSide.B_TO_A)
            {
                // If Not A=>B and B=>A
                MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_TRADE_WITHOUT_AUTO_SIDE"),
                    MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (isRandomMineAmount && !randomMineAmountToUnlimited)
            {
                //When mine amount is random and mine amount to is unlimited, need to check mine amount to is over mine amount
                if (randomMineAmounTo < mineAmount)
                {
                    MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_RANDOM_MINE_AMOUNT_TO_UNDER_FROM"),
                        MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
            }
            if (!maxMineAmountUnlimited)
            {
                //When max mine amount is not unlimited, need to check max mine amount is over minimum amount and is over mine amount
                //To ensure trading successfully, check all sell and buy price and amount is over
                if (mineAmountUnlimited)
                {
                    if (dragonExApiForA.Tde.MaxBid.Price * maxMineAmount <= ConfigTool.MinimumTradeUsdtAmount
                       || dragonExApiForA.Tde.MinAsk.Price * maxMineAmount <= ConfigTool.MinimumTradeUsdtAmount)
                    {
                        MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MAX_MINE_AMOUNT_UNDER_MINIMUM")
                            + Environment.NewLine + MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MINE_AMOUNT_ENSURE"),
                            MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                }
                else
                {
                    if (maxMineAmount < mineAmount)
                    {
                        MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MAX_MINE_AMOUNT_UNDER_MINE_AMOUNT")
                            + Environment.NewLine + MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_MINE_AMOUNT_ENSURE"),
                            MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                    if (isRandomMineAmount && !randomMineAmountToUnlimited)
                    {
                        if (randomMineAmounTo > maxMineAmount)
                        {
                            MessageBox.Show(MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_RANDOM_MINE_AMOUNT_TO_UNDER_FROM"),
                                MultiLanConfig.Instance.GetKeyValue("TITLE_INCORECT_PARAMETERS"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            return;
                        }
                    }
                }
            }
            tradeBtn.Enabled = false;
            logTxt.Text = "";
            lastTradeTime = 0;
            ExecTrading();
            if (tradingThread == null || !tradingThread.IsAlive)
            {
                tradeBtn.Enabled = true;
            }
        }

        private void aToBSideRb_CheckedChanged(object sender, EventArgs e)
        {
            if (aToBSideRb.Checked)
            {
                tradeSide = TradeSide.A_TO_B;
                tradeMethod = TradeMethod.A_TO_B;
            }
        }

        private void aBAFSideRb_CheckedChanged(object sender, EventArgs e)
        {
            if (aBAFSideRb.Checked)
            {
                tradeSide = TradeSide.BOTH_A_B_A_FIRST;
                tradeMethod = TradeMethod.A_TO_B;
            }
        }

        private void bToASideRb_CheckedChanged(object sender, EventArgs e)
        {
            if (bToASideRb.Checked)
            {
                tradeSide = TradeSide.B_TO_A;
                tradeMethod = TradeMethod.B_TO_A;
            }
        }

        private void aBBFSideRb_CheckedChanged(object sender, EventArgs e)
        {
            if (aBBFSideRb.Checked)
            {
                tradeSide = TradeSide.BOTH_A_B_B_FIRST;
                tradeMethod = TradeMethod.B_TO_A;
            }
        }

        private delegate void ResetTradingInfos(string logMsg, bool isEnd = false);

        /// <summary>
        /// Reset trading log
        /// </summary>
        /// <param name="logMsg">Log message</param>
        /// <param name="isEnd">Whether is end</param>
        private void ResetTradingLog(string logMsg, bool isEnd = false)
        {
            if (string.IsNullOrEmpty(logTxt.Text))
            {
                logTxt.Text = logMsg;
            }
            else
            {
                logTxt.Text = logTxt.Text + Environment.NewLine + logMsg;
            }
            if (isEnd)
            {
                if (!autoTrading)
                {
                    tradeBtn.Enabled = true;
                }
            }
        }

        /// <summary>
        /// Execute trading A=>B
        /// </summary>
        public void ExecTradingAB()
        {
            try
            {
                isTrading = true;
                var logMsg = string.Format(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_START_A"),
                    DateTime.Now.ToString("yyyyMMdd HHmmss") + Environment.NewLine, 
                    tradingEntity.Sell.ToString(),
                    tradingEntity.Amount.ToString());
                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                //Start A Sell
                var qte = dragonExApiForA.TradeCoinToBase(ConfigTool.CurrentPair.Value,
                    tradingEntity.Sell, tradingEntity.Amount);
                if (qte != null)
                {
                    dragonExApiForA.UserAmounts.CoinAmount -= CommonUtils.GetTruncateDecimal(tradingEntity.Amount, ConfigTool.Digits);
                    dragonExApiForA.UserAmounts.BaseAmount += CommonUtils.GetTruncateDecimal(
                        tradingEntity.Amount * tradingEntity.Sell * (1 - ConfigTool.TradeFee), ConfigTool.Digits);
                    this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });

                    logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_A_SELL_SUCCEED") + Environment.NewLine
                        + string.Format(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_START_B") , 
                        tradingEntity.Buy.ToString(), 
                        tradingEntity.Amount.ToString());
                    LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                    this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                    //Start B Buy
                    var te = dragonExApiForB.TradeBaseToCoin(ConfigTool.CurrentPair.Value, 
                        tradingEntity.Buy, tradingEntity.Amount);

                    if (te == null)
                    {
                        for (int tryCount = 1; tryCount <= ConfigTool.TradeTryCountWhenSellSucceed; tryCount++)
                        {
                            te = dragonExApiForB.TradeBaseToCoin(ConfigTool.CurrentPair.Value, 
                                tradingEntity.Buy, tradingEntity.Amount);
                            if (te != null)
                            {
                                logMsg = string.Format(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_B_BUY_ERROR_TRY_SUCCEED") , tryCount.ToString());
                                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                                break;
                            }
                            else
                            {
                                logMsg = string.Format(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_B_BUY_ERROR_TRY_FAILED"), tryCount.ToString());
                                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                            }
                            Thread.Sleep(200);
                        }
                    }

                    if (te == null)
                    {
                        // Cancel the sell order if all buy try is failed.
                        if (dragonExApiForA.CancelOrder(qte.OrderId))
                        {
                            dragonExApiForA.UserAmounts.CoinAmount += CommonUtils.GetTruncateDecimal(tradingEntity.Amount, ConfigTool.Digits);
                            dragonExApiForA.UserAmounts.BaseAmount -= CommonUtils.GetTruncateDecimal(
                                tradingEntity.Amount * tradingEntity.Sell * (1 - ConfigTool.TradeFee), ConfigTool.Digits);
                            this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });

                            logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_CANCEL_A_SUCCEED");
                            LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                            this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                        }
                        else
                        {
                            // Cancel failed.Add lost amount.
                            currentTradeLostCoinAmount += tradingEntity.Amount;

                            logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_CANCEL_A_FAILED");
                            LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                            this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                        }
                    }

                    if (te != null)
                    {
                        logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_B_BUY_SUCCEED");
                        LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                        this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });

                        dragonExApiForB.UserAmounts.CoinAmount += CommonUtils.GetTruncateDecimal(tradingEntity.Amount, ConfigTool.Digits);
                        dragonExApiForB.UserAmounts.BaseAmount -= CommonUtils.GetTruncateDecimal(
                            tradingEntity.Amount * tradingEntity.Buy * (1 + ConfigTool.TradeFee), ConfigTool.Digits);
                        this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });

                        //Wait 5s for trading succeed
                        Thread.Sleep(5000);
                        //try 5 count check
                        bool aResult, bResult;
                        aResult = false;
                        bResult = false;
                        for (int tryCount = 1; tryCount <= ConfigTool.ConfirmTryCountForTradeSucceed; tryCount++)
                        {
                            if (!aResult)
                            {
                                aResult = dragonExApiForA.CheckOrderSucceed(qte.OrderId);
                                if (aResult)
                                {
                                    logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_CHECK_A_SUCCEED");
                                    this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                                }
                            }
                            if (!bResult)
                            {
                                bResult = dragonExApiForB.CheckOrderSucceed(te.OrderId);
                                if (bResult)
                                {
                                    logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_CHECK_B_SUCCEED");
                                    this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                                }
                            }
                            if (aResult && bResult)
                            {
                                //If all succeeded, exit try
                                break;
                            }
                            Thread.Sleep(2000);
                        }

                        if (aResult && !bResult)
                        {
                            // Cancel the buy order
                            if (dragonExApiForB.CancelOrder(te.OrderId))
                            {
                                dragonExApiForB.UserAmounts.CoinAmount -= CommonUtils.GetTruncateDecimal(tradingEntity.Amount, ConfigTool.Digits);
                                dragonExApiForB.UserAmounts.BaseAmount += CommonUtils.GetTruncateDecimal(
                                    tradingEntity.Amount * tradingEntity.Buy * (1 + ConfigTool.TradeFee), ConfigTool.Digits);
                                this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });

                                // Cancel Succeed.Add lost amount.
                                currentTradeLostCoinAmount += tradingEntity.Amount;

                                logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_CANCEL_B_SUCCEED");
                                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                            }
                            else
                            {
                                logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_CANCEL_B_FAILED");
                                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                            }
                        }
                        else if(bResult && !aResult)
                        {
                            // Cancel the sell order
                            if (dragonExApiForA.CancelOrder(qte.OrderId))
                            {
                                dragonExApiForA.UserAmounts.CoinAmount += CommonUtils.GetTruncateDecimal(tradingEntity.Amount, ConfigTool.Digits);
                                dragonExApiForA.UserAmounts.BaseAmount -= CommonUtils.GetTruncateDecimal(
                                    tradingEntity.Amount * tradingEntity.Sell * (1 - ConfigTool.TradeFee), ConfigTool.Digits);
                                this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });

                                // Cancel Succeed.Add plus amount.
                                currentTradePlusCoinAmount += tradingEntity.Amount;

                                logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_CANCEL_A_SUCCEED");
                                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                            }
                            else
                            {
                                logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_CANCEL_A_FAILED");
                                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                            }
                        }

                        Thread.Sleep(2000);
                        GetUserInfo();

                        currentSideMineAmountTotal += tradingEntity.Amount;

                        logMsg = "";
                        this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, true });
                    }
                    else
                    {
                        logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_B_BUY_FAILED");
                        LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                        this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, true });
                    }
                }
                else
                {
                    logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_AB_A_SELL_FAILED");
                    LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                    this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, true });
                }
            }
            catch (ThreadAbortException)
            {
                //Do nothing when aborting thread
                var logMsg = MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_TRADING_AB_TREAD_ABORTED");
                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, true });
            }
            catch (Exception ex)
            {
                //Error happened when getting
                var logMsg = MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_TRADING_AB_EXCEPTION_HAPPEND") 
                    + Environment.NewLine
                    + ex.Message + Environment.NewLine + ex.StackTrace;
                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, true });
            }
            finally
            {
                lastTradeTime = DateTime.Now.Ticks;
                isTrading = false;
                this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });
            }
        }

        /// <summary>
        /// Execute trading B=>A
        /// </summary>
        public void ExecTradingBA()
        {
            try
            {
                isTrading = true;
                var logMsg = string.Format(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_START_B") ,
                    DateTime.Now.ToString("yyyyMMdd HHmmss") + Environment.NewLine,
                    tradingEntity.Sell.ToString(),
                    tradingEntity.Amount.ToString());
                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                //Start B Sell
                var te = dragonExApiForB.TradeCoinToBase(ConfigTool.CurrentPair.Value, 
                    tradingEntity.Sell, tradingEntity.Amount);
                if (te != null)
                {
                    dragonExApiForB.UserAmounts.CoinAmount -= CommonUtils.GetTruncateDecimal(tradingEntity.Amount, ConfigTool.Digits);
                    dragonExApiForB.UserAmounts.BaseAmount += CommonUtils.GetTruncateDecimal(
                        tradingEntity.Amount * tradingEntity.Sell * (1 - ConfigTool.TradeFee), ConfigTool.Digits);
                    this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });

                    logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_B_SELL_SUCCEED") + Environment.NewLine
                        + string.Format(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_START_A") ,
                        tradingEntity.Buy.ToString(),
                        tradingEntity.Amount.ToString());
                    LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                    this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                    //Start A Buy
                    var qte = dragonExApiForA.TradeBaseToCoin(ConfigTool.CurrentPair.Value,
                        tradingEntity.Buy, tradingEntity.Amount);

                    if (qte == null)
                    {
                        for (int tryCount = 1; tryCount <= ConfigTool.TradeTryCountWhenSellSucceed; tryCount++)
                        {
                            qte = dragonExApiForA.TradeBaseToCoin(ConfigTool.CurrentPair.Value, 
                                tradingEntity.Buy, tradingEntity.Amount);
                            if (qte != null)
                            {
                                logMsg = string.Format(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_A_BUY_ERROR_TRY_SUCCEED"),
                                    tryCount.ToString());
                                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                                break;
                            }
                            else
                            {
                                logMsg = string.Format(MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_A_BUY_ERROR_TRY_FAILED") ,
                                    tryCount.ToString());
                                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                            }
                            Thread.Sleep(200);
                        }
                    }

                    if (qte == null)
                    {
                        // Cancel the sell order if all buy try is failed.
                        if (dragonExApiForB.CancelOrder(te.OrderId))
                        {
                            dragonExApiForB.UserAmounts.CoinAmount += CommonUtils.GetTruncateDecimal(tradingEntity.Amount, ConfigTool.Digits);
                            dragonExApiForB.UserAmounts.BaseAmount -= CommonUtils.GetTruncateDecimal(
                                tradingEntity.Amount * tradingEntity.Sell * (1 - ConfigTool.TradeFee), ConfigTool.Digits);
                            this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });

                            logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_CANCEL_B_SUCCEED");
                            LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                            this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                        }
                        else
                        {
                            // Cancel failed.Add lost amount.
                            currentTradeLostCoinAmount += tradingEntity.Amount;

                            logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_CANCEL_B_FAILED");
                            LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                            this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                        }
                    }

                    if (qte != null)
                    {
                        logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_A_BUY_SUCCEED");
                        LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                        this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });

                        dragonExApiForA.UserAmounts.CoinAmount += CommonUtils.GetTruncateDecimal(tradingEntity.Amount, ConfigTool.Digits);
                        dragonExApiForA.UserAmounts.BaseAmount -= CommonUtils.GetTruncateDecimal(
                            tradingEntity.Amount * tradingEntity.Buy * (1 + ConfigTool.TradeFee), ConfigTool.Digits);
                        this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });

                        //try * count check
                        bool aResult, bResult;
                        aResult = false;
                        bResult = false;
                        for (int tryCount = 1; tryCount <= ConfigTool.ConfirmTryCountForTradeSucceed; tryCount++)
                        {
                            Thread.Sleep(2000);
                            if (!bResult)
                            {
                                bResult = dragonExApiForB.CheckOrderSucceed(te.OrderId);
                                if (bResult)
                                {
                                    logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_CHECK_B_SUCCEED");
                                    this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                                }
                            }
                            if (!aResult)
                            {
                                aResult = dragonExApiForA.CheckOrderSucceed(qte.OrderId);
                                if (aResult)
                                {
                                    logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_CHECK_A_SUCCEED");
                                    this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                                }
                            }
                            if (aResult && bResult)
                            {
                                //If all succeeded, exit try
                                break;
                            }
                        }

                        if (aResult && !bResult)
                        {
                            // Cancel the sell order
                            if (dragonExApiForB.CancelOrder(te.OrderId))
                            {
                                dragonExApiForB.UserAmounts.CoinAmount += CommonUtils.GetTruncateDecimal(tradingEntity.Amount, ConfigTool.Digits);
                                dragonExApiForB.UserAmounts.BaseAmount -= CommonUtils.GetTruncateDecimal(
                                    tradingEntity.Amount * tradingEntity.Sell * (1 - ConfigTool.TradeFee), ConfigTool.Digits);
                                this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });

                                // Cancel Succeed.Add plus amount.
                                currentTradePlusCoinAmount += tradingEntity.Amount;

                                logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_CANCEL_B_SUCCEED");
                                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                            }
                            else
                            {
                                logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_CANCEL_B_FAILED");
                                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                            }
                        }
                        else if (bResult && !aResult)
                        {
                            // Cancel the buy order
                            if (dragonExApiForA.CancelOrder(qte.OrderId))
                            {
                                dragonExApiForA.UserAmounts.CoinAmount -= CommonUtils.GetTruncateDecimal(tradingEntity.Amount, ConfigTool.Digits);
                                dragonExApiForA.UserAmounts.BaseAmount += CommonUtils.GetTruncateDecimal(
                                    tradingEntity.Amount * tradingEntity.Buy * (1 + ConfigTool.TradeFee), ConfigTool.Digits);
                                this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });

                                // Cancel Succeed.Add lost amount.
                                currentTradeLostCoinAmount += tradingEntity.Amount;

                                logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_CANCEL_A_SUCCEED");
                                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                            }
                            else
                            {
                                logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_CANCEL_A_FAILED");
                                LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, false });
                            }
                        }

                        Thread.Sleep(2000);
                        GetUserInfo();

                        currentSideMineAmountTotal += tradingEntity.Amount;

                        logMsg = "";
                        this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, true });
                    }
                    else
                    {
                        logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_A_BUY_FAILED");
                        LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                        this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, true });
                    }
                }
                else
                {
                    logMsg = MultiLanConfig.Instance.GetKeyValue("INFO_TRADING_BA_B_SELL_FAILED");
                    LogTool.LogTradeInfo(logMsg, LogLevels.TRACE);
                    this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, true });
                }
            }
            catch (ThreadAbortException)
            {
                //Do nothing when aborting thread
                var logMsg = MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_TRADING_BA_TREAD_ABORTED");
                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, true });
            }
            catch (Exception ex)
            {
                //Error happened when getting
                var logMsg = MultiLanConfig.Instance.GetKeyValue("ERROR_MSG_TRADING_BA_EXCEPTION_HAPPEND") 
                    + Environment.NewLine
                    + ex.Message + Environment.NewLine + ex.StackTrace;
                Console.WriteLine(logMsg);
                LogTool.LogTradeInfo(logMsg, LogLevels.ERROR);
                this.Invoke(new ResetTradingInfos(ResetTradingLog), new object[] { logMsg, true });
            }
            finally
            {
                lastTradeTime = DateTime.Now.Ticks;
                isTrading = false;
                this.Invoke(new ResetUI(ResetCurrentUserInfos), new object[] { });
            }
        }

        private void maxMineAmountUnlimitedChk_CheckedChanged(object sender, EventArgs e)
        {
            maxMineAmountUnlimited = maxMineAmountUnlimitedChk.Checked;
            if (!maxMineAmountUnlimited)
            {
                maxMineAmountNud.Value = previousMaxMineAmount;
            }
            maxMineAmountNud.Enabled = !maxMineAmountUnlimited;
        }

        private void maxMineAmountNud_ValueChanged(object sender, EventArgs e)
        {
            if (!maxMineAmountUnlimited)
            {
                maxMineAmount = maxMineAmountNud.Value;
                previousMaxMineAmount = maxMineAmount;
            }
        }

        private void randomMineAmountChk_CheckedChanged(object sender, EventArgs e)
        {
            mineAmountUnlimitedChk.Enabled = !randomMineAmountChk.Checked;
            if (randomMineAmountChk.Checked && mineAmountUnlimitedChk.Checked)
            {
                mineAmountUnlimitedChk.Checked = false;
            }
            mineAmountToNud.Enabled = randomMineAmountChk.Checked && !mineAmountToUnlimitedChk.Checked;
            mineAmountToUnlimitedChk.Enabled = randomMineAmountChk.Checked;
            isRandomMineAmount = randomMineAmountChk.Checked;
        }

        private void mineAmountToNud_ValueChanged(object sender, EventArgs e)
        {
            if (!randomMineAmountToUnlimited)
            {
                randomMineAmounTo = mineAmountToNud.Value;
                previousRndomMineAmounTo = randomMineAmounTo;
            }
        }

        private void mineAmountToUnlimitedChk_CheckedChanged(object sender, EventArgs e)
        {
            randomMineAmountToUnlimited = mineAmountToUnlimitedChk.Checked;
            if (!randomMineAmountToUnlimited)
            {
                mineAmountToNud.Value = previousRndomMineAmounTo;
            }
            mineAmountToNud.Enabled = !randomMineAmountToUnlimited;
        }

        /// <summary>
        /// Lost coin limit unlimited
        /// </summary>
        private bool lostCoinLimitUnlimited = true;

        /// <summary>
        /// Lost coin limit
        /// </summary>
        private decimal lostCoinLimit = 100.0m;

        /// <summary>
        /// Previous Lost coin limit
        /// </summary>
        private decimal previousLostCoinLimit = 100.0m;

        /// <summary>
        /// Current trade lost coin amount
        /// </summary>
        private decimal currentTradeLostCoinAmount = 0.0m;

        /// <summary>
        /// Current trade plus coin amount
        /// </summary>
        private decimal currentTradePlusCoinAmount = 0.0m;

        private void lostCoinLimitUnlimitedChk_CheckedChanged(object sender, EventArgs e)
        {
            lostCoinLimitUnlimited = lostCoinLimitUnlimitedChk.Checked;
            if (!lostCoinLimitUnlimited)
            {
                lostCoinLimitNud.Value = previousLostCoinLimit;
            }
            lostCoinLimitNud.Enabled = !lostCoinLimitUnlimited;
        }

        private void lostCoinLimitNud_ValueChanged(object sender, EventArgs e)
        {
            if (!lostCoinLimitUnlimited)
            {
                lostCoinLimit = lostCoinLimitNud.Value;
                previousLostCoinLimit = lostCoinLimit;
            }
        }

        private void tradeBaseToCoinFailedChk_CheckedChanged(object sender, EventArgs e)
        {
            TestConfigTool.TradeBaseToCoinFailed = tradeBaseToCoinFailedChk.Checked;
        }

        private void tradeCoinToBaseFailed_CheckedChanged(object sender, EventArgs e)
        {
            TestConfigTool.TradeCoinToBaseFailed = tradeCoinToBaseFailed.Checked;
        }

        private void checkAOrderFailedChk_CheckedChanged(object sender, EventArgs e)
        {
            TestConfigTool.CheckAOrderFailed = checkAOrderFailedChk.Checked;
        }

        private void checkBOrderFailedChk_CheckedChanged(object sender, EventArgs e)
        {
            TestConfigTool.CheckBOrderFailed = checkBOrderFailedChk.Checked;
        }

        private void cancelAOrderFailedChk_CheckedChanged(object sender, EventArgs e)
        {
            TestConfigTool.CancelAOrderFailed = cancelAOrderFailedChk.Checked;
        }

        private void cancelBOrderFailedChk_CheckedChanged(object sender, EventArgs e)
        {
            TestConfigTool.CancelBOrderFailed = cancelBOrderFailedChk.Checked;
        }
    }
}
