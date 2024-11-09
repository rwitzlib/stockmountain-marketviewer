using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestEntryStats
{
    public float PositiveTrendRatio { get; set; }
    public float AvgWin { get; set; }
    public float AvgLoss { get; set; }
    public float AvgProfit { get; set; }
    public float SumProfit { get; set; }
}
