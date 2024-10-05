using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestEntryV3
{
    public Guid EntryId { get; set; }
    public DateTime Date { get; set; }

    /// <summary>
    /// Cost is $0.000133334 per credit
    /// </summary>
    public float CreditsUsed { get; set; }
    public BackTestEntryStatsV3 Hold { get; set; }
    public BackTestEntryStatsV3 High { get; set; }
    public List<BackTestEntryResultCollection> Results { get; set; }
}

[ExcludeFromCodeCoverage]
public class BackTestEntryStatsV3
{
    public float PositiveTrendRatio { get; set; }
    public float AvgWin { get; set; }
    public float AvgLoss { get; set; }
    public float AvgProfit { get; set; }
    public float SumProfit { get; set; }
}

[ExcludeFromCodeCoverage]
public class BackTestEntryResultCollection
{
    public string Ticker { get; set; }
    public DateTimeOffset BoughtAt { get; set; }
    public float StartPrice { get; set; }
    public int Shares { get; set; }
    public float StartPosition { get; set; }
    public BackTestEntryResultV3 Hold { get; set; }
    public BackTestEntryResultV3 High { get; set; }
}

[ExcludeFromCodeCoverage]
public class BackTestEntryResultV3
{
    public DateTimeOffset SoldAt { get; set; }
    public float EndPrice { get; set; }
    public float EndPosition { get; set; }
    public float Profit { get; set; }
    public bool StoppedOut { get; set; }
}