using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Models.Backtest;

namespace MarketViewer.Contracts.Responses.Backtest
{
    [ExcludeFromCodeCoverage]
    public class BacktestV2Response
    {
        public string Id { get; set; }
        public BackTestEntryStats Hold { get; set; }
        public BackTestEntryStats High { get; set; }
        public IEnumerable<BacktestEntryV2> Results { get; set; }
    }
}
