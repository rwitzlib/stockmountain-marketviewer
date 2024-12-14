using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.ScanV2.Operands;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.ScanV2;

[ExcludeFromCodeCoverage]
public class PriceActionOperand : IScanOperand
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PriceActionType PriceAction { get; set; }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OperandModifier Modifier { get; set; }

    public int Multiplier { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Timespan Timespan { get; set; }

    public int GetPriority()
    {
        return 50;
    }

    public bool HasTimeframe(out int? multiplier, out Timespan? timespan)
    {
        multiplier = Multiplier;
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