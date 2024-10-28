using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Models.Backtest;

namespace MarketViewer.Contracts.Responses.Backtest
{
    [ExcludeFromCodeCoverage]
    public class BacktestV2Response
    {
        public string Id { get; set; }
        public BackTestEntryStatsV2 Hold { get; set; }
        public BackTestEntryStatsV2 High { get; set; }
        public IEnumerable<BacktestEntryV2> Results { get; set; }
    }
}
