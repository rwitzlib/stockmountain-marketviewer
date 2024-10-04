using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestPosition
{
    public float StartingBalance { get; set; }
    public int MaxConcurrentPositions { get; set; }
    public float PositionSize { get; set; }
}
