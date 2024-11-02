using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Responses.Backtest
{
    [ExcludeFromCodeCoverage]
    public class BacktestResponseV2
    {
        public string Id { get; set; }
        public BackTestEntryStatsV2 Hold { get; set; }
        public BackTestEntryStatsV2 High { get; set; }
        public IEnumerable<BacktestEntryV2> Results { get; set; }
    }
}
