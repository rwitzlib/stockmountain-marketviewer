using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Responses.Backtest
{
    [ExcludeFromCodeCoverage]
    public class BacktestResponseV2
    {
        public string Id { get; set; }
        public BackTestEntryStatsV2 Hold { get; set; }
        public BackTestEntryStatsV2 High { get; set; }
        public IEnumerable<BacktestLambdaResponseV2> Results { get; set; }
    }
}
