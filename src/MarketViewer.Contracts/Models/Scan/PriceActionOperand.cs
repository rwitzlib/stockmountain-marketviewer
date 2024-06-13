using MarketViewer.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using Timespan = MarketViewer.Contracts.Enums.Timespan;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class PriceActionOperand : IScanOperand
{
    public Modifier ValueType { get; set; }
    public PriceActionType Type { get; set; }
    public int Multiplier { get; set; }
    public Timespan Timespan { get; set; }

    public float[] Compute(StocksResponse stocksResponse, Timeframe timeframe)
    {
        throw new System.NotImplementedException();
    }

    public bool HasTimespan(out Timespan? timespan)
    {
        timespan = Timespan;
        return true;
    }
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
