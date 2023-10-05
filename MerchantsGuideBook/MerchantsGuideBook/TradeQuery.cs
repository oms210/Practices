using System;
using System.Collections.Generic;
using System.Text;

namespace MerchantsGuideBook
{
    internal class TradeQuery
    {
        internal TradeQuery()
        { }
        internal int TradeID { get; set; }
        internal string  Query { get; set; }
        internal int ArabicNumber { get; set; }
        internal string Product { get; set; }
        internal int Credits { get; set; }
    }
}
