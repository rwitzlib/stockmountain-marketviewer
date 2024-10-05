using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Models.Backtest;

namespace MarketViewer.Contracts.Responses.Backtest
{
    [ExcludeFromCodeCoverage]
    public class BacktestV3Response
    {
        public string Id { get; set; }
        public float HoldBalance { get; set; }
        public float HighBalance { get; set; }
        public BackTestEntryStatsV3 Hold { get; set; }
        public BackTestEntryStatsV3 High { get; set; }
        public IEnumerable<BacktestEntryV3> Results { get; set; }
    }
}
