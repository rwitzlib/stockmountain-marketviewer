using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.BacktestV2;

[ExcludeFromCodeCoverage]
public class Feature
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FeatureType Type { get; set; }

    public string Value { get; set; }
}
