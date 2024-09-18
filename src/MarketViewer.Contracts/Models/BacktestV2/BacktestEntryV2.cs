using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.BacktestV2;

[ExcludeFromCodeCoverage]
public class BacktestEntryV2
{
    public Guid EntryId { get; set; }
    public DateTime Date { get; set; }
    public BackTestEntryStats Hold { get; set; }
    public BackTestEntryStats High { get; set; }
    public BackTestEntryStats Other { get; set; }
    public List<List<BackTestEntryResult>> Results { get; set; } 
}

[ExcludeFromCodeCoverage]
public class BackTestEntryStats
{
    public float PositiveTrendRatio { get; set; }
    public float HighPosition { get; set; }
    public float LowPosition { get; set; }
    public float AvgProfit { get; set; }
    public float SumProfit { get; set; }
}

[ExcludeFromCodeCoverage]
public class BackTestEntryResult
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