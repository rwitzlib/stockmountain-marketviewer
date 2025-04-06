using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Entities.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestEntryStats
{
    public float EndBalance { get; set; }
    public float BalanceChange { get; set; }
    public float SumProfit { get; set; }
    public float WinRatio { get; set; }
    public float AvgWin { get; set; }
    public float AvgLoss { get; set; }
    public float MaxConcurrentPositions { get; set; }
}
