using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Timespan
{
    minute,
    hour,
    day,
    week,
    month,
    quarter,
    year
}
