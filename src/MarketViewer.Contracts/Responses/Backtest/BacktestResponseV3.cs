using MarketViewer.Contracts.Models.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Responses.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestResponseV3
{
    public string Id { get; set; }
    public float HoldBalance { get; set; }
    public float HighBalance { get; set; }
    public int MaxConcurrentPositions { get; set; }
    public BackTestEntryStatsV3 Hold { get; set; }
    public BackTestEntryStatsV3 High { get; set; }
    public IEnumerable<BacktestDayResultV3> Results { get; set; }
    public IEnumerable<BacktestLambdaResponseV3> Other { get; set; }
}