using MarketViewer.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketViewer.Contracts.Interfaces
{
    internal interface IMarketCache
    {
        Task Initialize(DateTime datetime, int multiplier, Timespan timespan);


    }
}
