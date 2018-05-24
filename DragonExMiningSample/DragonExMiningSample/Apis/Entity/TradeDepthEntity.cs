using DragonExMiningSampleCSharp.Apis.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonExMiningSampleCSharp.Apis.Entity
{
    class TradeDepthEntity
    {
        private AskBidEntity minAsk1;
        private AskBidEntity maxBid1;
        private AskBidEntity minAsk2;
        private AskBidEntity maxBid2;
        private AskBidEntity minAsk3;
        private AskBidEntity maxBid3;

        public string TradeName
        {
            get;
            set;
        }

        public List<AskBidEntity> AsksList
        {
            get;
            set;
        }

        public List<AskBidEntity> BidsList
        {
            get;
            set;
        }

        public AskBidEntity MinAsk
        {
            get
            {
                AskBidEntity currentMinAsk = null;
                switch (ConfigTool.UseCurrentAskBidIndex)
                {
                    case 1:
                        currentMinAsk = minAsk1;
                        break;
                    case 2:
                        currentMinAsk = minAsk2;
                        break;
                    case 3:
                        currentMinAsk = minAsk3;
                        break;
                }
                return currentMinAsk;
            }
            set
            {
                minAsk1 = value;
            }
        }

        public decimal MinAskBZableAmount
        {
            get
            {
                decimal currentMinAsk = 0.0m;
                switch (ConfigTool.UseCurrentAskBidIndex)
                {
                    case 1:
                        currentMinAsk = minAsk1.Amount;
                        break;
                    case 2:
                        currentMinAsk = (minAsk2.Amount + minAsk1.Amount) * 0.8m;
                        break;
                    case 3:
                        currentMinAsk = (minAsk3.Amount + minAsk2.Amount + minAsk1.Amount) * 0.8m;
                        break;
                }
                return currentMinAsk;
            }
        }

        public AskBidEntity MaxBid
        {
            get
            {
                AskBidEntity currentMaxBid = null;
                switch (ConfigTool.UseCurrentAskBidIndex)
                {
                    case 1:
                        currentMaxBid = maxBid1;
                        break;
                    case 2:
                        currentMaxBid = maxBid2;
                        break;
                    case 3:
                        currentMaxBid = maxBid3;
                        break;
                }
                return currentMaxBid;
            }
            set
            {
                maxBid1 = value;
            }
        }

        public decimal MaxBidBZableAmount
        {
            get
            {
                decimal currentMaxBid = 0.0m;
                switch (ConfigTool.UseCurrentAskBidIndex)
                {
                    case 1:
                        currentMaxBid = maxBid1.Amount;
                        break;
                    case 2:
                        currentMaxBid = (maxBid2.Amount + maxBid1.Amount) * 0.8m;
                        break;
                    case 3:
                        currentMaxBid = (maxBid3.Amount + maxBid2.Amount + maxBid1.Amount) * 0.8m;
                        break;
                }
                return currentMaxBid;
            }
        }

        public AskBidEntity MinSecAsk
        {
            get
            {
                return minAsk2;
            }
            set
            {
                minAsk2 = value;
            }
        }

        public AskBidEntity MaxSecBid
        {
            get
            {
                return maxBid2;
            }
            set
            {
                maxBid2 = value;
            }
        }

        public AskBidEntity MinThdAsk
        {
            get
            {
                return minAsk3;
            }
            set
            {
                minAsk3 = value;
            }
        }

        public AskBidEntity MaxThdBid
        {
            get
            {
                return maxBid3;
            }
            set
            {
                maxBid3 = value;
            }
        }
    }
}
