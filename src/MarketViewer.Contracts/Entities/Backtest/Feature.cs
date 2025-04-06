using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Entities.Backtest;

/// <summary>
/// 
/// </summary>
[ExcludeFromCodeCoverage]
public class Feature
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FeatureType Type { get; set; }

    public string Value { get; set; }
}

/// <summary>
/// Types of available features available for backtesting
/// </summary>
public enum FeatureType
{
    Ticker,
    EntryBefore,
    EntryAfter,
    TickerType,
    DayOfWeek
}
