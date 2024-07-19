using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.ScanV2;

[ExcludeFromCodeCoverage]
public class PriceActionOperand : IScanOperand
{
    public string Name { get; } = "PriceAction";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OperandModifier Modifier { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PriceActionType Type { get; set; }

    public int Multiplier { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Timespan Timespan { get; set; }

    public bool HasTimespan(out Timespan? timespan)
    {
        timespan = Timespan;
        return true;
    }
}

public enum PriceActionType
{
    Open,
    Close,
    High,
    Low,
    Vwap,
    Volume
}