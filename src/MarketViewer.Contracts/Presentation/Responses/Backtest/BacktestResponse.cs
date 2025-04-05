using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Responses.Backtest
{
    [ExcludeFromCodeCoverage]
    public class BacktestResponse
    {
        public Guid RequestId { get; set; }
        public IEnumerable<BacktestLambdaResponse> Results { get; set; }
        public int ResultsCount { get; set; }
        public double LongRatioAvg { get; set; }
        public double ShortRatioAvg { get; set; }
        public float LongPositionAvgChange { get; set; }
        public float ShortPositionAvgChange { get; set; }

    }
}
