using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Models.Backtest;

namespace MarketViewer.Contracts.Responses.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestV3Response
{
    public string Id { get; set; }
    public float HoldBalance { get; set; }
    public float HighBalance { get; set; }
    public int MaxConcurrentPositions { get; set; }
    public BackTestEntryStatsV3 Hold { get; set; }
    public BackTestEntryStatsV3 High { get; set; }
    public IEnumerable<BacktestDayV3> Results { get; set; }
    public IEnumerable<BacktestEntryV3> Other { get; set; }
}

[ExcludeFromCodeCoverage]
public class BacktestDayV3
{
    public DateTimeOffset Date { get; set; }
    public float CreditsUsed { get; set; }
    public BacktestDayDetails Hold { get; set; }
    public BacktestDayDetails High { get; set; }
}

[ExcludeFromCodeCoverage]
public class BacktestDayDetails
{
    public float StartingBalance { get; set; }
    public float EndingBalance { get; set; }
    public float Profit { get; set; }
    public int OpenPositions { get; set; }
    public List<BacktestDayPosition> Bought { get; set; }
    public List<BacktestDayPosition> Sold { get; set; }
}

[ExcludeFromCodeCoverage]
public class BacktestDayPosition
{
    public string Ticker { get; set; }
    public float Price { get; set; }
    public int Shares { get; set; }
    public float Position { get; set; }
    public float Profit { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public bool StoppedOut { get; set; }
}