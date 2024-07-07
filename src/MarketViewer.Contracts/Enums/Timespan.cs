using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Timespan
{
    minute,
    hour = 60,
    day,
    week,
    month,
    quarter,
    year
}
