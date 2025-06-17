using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan.Operands;

[ExcludeFromCodeCoverage]
public class PriceActionOperand : IScanOperand
{
    public PriceActionType PriceAction { get; set; }
    public OperandModifier Modifier { get; set; }
    public Timeframe Timeframe { get; set; }

    public OperandType GetOperandType()
    {
        return OperandType.PriceAction;
    }

    public int GetPriority()
    {
        return 50;
    }

    public bool HasTimeframe(out Timeframe timeframe)
    {
        timeframe = Timeframe;
        return true;
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PriceActionType
{
    open,
    close,
    high,
    low,
    vwap,
    volume
}