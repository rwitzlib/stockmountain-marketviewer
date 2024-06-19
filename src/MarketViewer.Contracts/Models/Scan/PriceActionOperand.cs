using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses;
using System.Text.Json.Serialization;
using Timespan = MarketViewer.Contracts.Enums.Timespan;

namespace MarketViewer.Contracts.Models.Scan;

public class PriceActionOperand : IScanOperand
{
    [JsonRequired]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Modifier ValueType { get; set; }

    [JsonRequired]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PriceActionType Type { get; set; }

    [JsonRequired]
    public int Multiplier { get; set; }

    [JsonRequired]
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

public enum Modifier
{
    Value,
    Slope
}


