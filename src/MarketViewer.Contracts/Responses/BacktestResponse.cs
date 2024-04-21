using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Responses
{
    [ExcludeFromCodeCoverage]
    public class BacktestResponse
    {
        public Guid RequestId { get; set; }
        public IEnumerable<BacktestEntry> Results { get; set; }
        public int ResultsCount { get; set; }
        public double LongRatioAvg { get; set; }
        public double ShortRatioAvg { get; set; }
        public float LongPositionAvgChange { get; set; }
        public float ShortPositionAvgChange { get; set; }

    }
}
