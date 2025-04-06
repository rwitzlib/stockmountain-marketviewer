using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Entities.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestEntryResult
{
    public DateTimeOffset SoldAt { get; set; }
    public float EndPrice { get; set; }
    public float EndPosition { get; set; }
    public float Profit { get; set; }
    public bool StoppedOut { get; set; }
}
