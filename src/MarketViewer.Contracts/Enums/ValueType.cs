using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<ValueType>))]
public enum ValueType
{
    percent,
    flat
}
