using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DragonExMiningSampleCSharp.Apis.Common
{
    public class CommonUtils
    {
        public static string GetAskBidElementValue(XNode node)
        {
            return ((XText)(((XElement)(node)).FirstNode)).Value;
        }

        public static decimal GetTruncateDecimal(decimal value, int decimals)
        {
            long xh = (long)Math.Pow(10, decimals);
            return decimal.Floor(value * xh) / xh;
        }
    }
}
