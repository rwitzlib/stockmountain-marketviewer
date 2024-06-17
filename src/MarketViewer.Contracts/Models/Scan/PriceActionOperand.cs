using MarketViewer.Contracts.Responses;
using System.Collections.Generic;
using System.Linq;
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

    public float[] Compute(StocksResponse stocksResponse, Timeframe timeframe)
    {
        var candles = Type switch
        {
            PriceActionType.Open => stocksResponse.Results.Select(q => q.Open),
            PriceActionType.Close => stocksResponse.Results.Select(q => q.Close),
            PriceActionType.High => stocksResponse.Results.Select(q => q.High),
            PriceActionType.Low => stocksResponse.Results.Select(q => q.Low),
            PriceActionType.Vwap => stocksResponse.Results.Select(q => q.Vwap),
            PriceActionType.Volume => stocksResponse.Results.Select(q => q.Volume),
            _ => [],
        };

        var values = ValueType switch
        {
            Modifier.Value => candles,
            Modifier.Slope => GetSlope(candles.ToArray()),
            _ => []
        };

        return values.TakeLast(timeframe.Multiplier).ToArray();
    }

    public bool HasTimespan(out Timespan? timespan)
    {
        timespan = Timespan;
        return true;
    }

    private static float[] GetSlope(float[] values)
    {
        if (values.Length < 2)
        {
            return [];
        }

        var results = new float [values.Length - 1];

        for (int i = 0; i < values.Length - 1; i++)
        {
            results[i] = values[i + 1] - values[i];
        }

        return results;
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


