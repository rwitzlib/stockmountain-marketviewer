using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Responses.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestLambdaResponseV2
{
    public Guid EntryId { get; set; }
    public DateTime Date { get; set; }

    /// <summary>
    /// Cost is $0.000133334 per credit
    /// </summary>
    public float CreditsUsed { get; set; }
    public BackTestEntryStatsV2 Hold { get; set; }
    public BackTestEntryStatsV2 High { get; set; }
    public List<List<BackTestEntryResultV2>> Results { get; set; }
}

[ExcludeFromCodeCoverage]
public class BackTestEntryStatsV2
{
    public float PositiveTrendRatio { get; set; }
    public float HighPosition { get; set; }
    public float LowPosition { get; set; }
    public float AvgProfit { get; set; }
    public float SumProfit { get; set; }
}

[ExcludeFromCodeCoverage]
public class BackTestEntryResultV2
{
    public string Name { get; set; }
    public string Ticker { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DayOfWeek Day { get; set; }
    public float Price { get; set; }
    public float Float { get; set; }
    public float Volume { get; set; }
    public float StartPosition { get; set; }
    public float LowPosition { get; set; }
    public float EndPosition { get; set; }
    public int Shares { get; set; }
    public float Profit { get; set; }
    public bool StoppedOut { get; set; }
}