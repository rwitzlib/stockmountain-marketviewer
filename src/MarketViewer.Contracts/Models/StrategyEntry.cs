using Polygon.Client.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models
{
    [ExcludeFromCodeCoverage]
    public class StrategyEntry
    {
        public string Ticker { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public IEnumerable<Bar> Bars { get; set; }
        public IEnumerable<string> Characteristics { get; set; }
    }
}
