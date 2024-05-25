using System.Diagnostics.CodeAnalysis;
using Timespan = MarketViewer.Contracts.Enums.Timespan;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class PriceActionOperand : IScanOperand
{
    public CandleAmount CandleAmount { get; set; }
    public int Multiplier { get; set; }
    public Timespan Timespan { get; set; }
    public PriceActionType Type { get; set; }
    public Modifier ValueType { get; set; }
}

public enum CandleAmount
{
    Any,
    All
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

public enum Modifier
{
    Value,
    Slope
}
