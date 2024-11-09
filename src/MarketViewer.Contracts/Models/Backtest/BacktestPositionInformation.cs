using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Backtest;

/// <summary>
/// asdf
/// </summary>
[ExcludeFromCodeCoverage]
public class BacktestPositionInformation
{
    public float StartingBalance { get; set; }
    public int MaxConcurrentPositions { get; set; }
    public float PositionSize { get; set; }
}
