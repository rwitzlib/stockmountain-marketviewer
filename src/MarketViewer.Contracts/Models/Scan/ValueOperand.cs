using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

public class ValueOperand : IScanOperand
{
    [JsonRequired]
    public float Value { get; set; }

    public float[] Compute(StocksResponse stocksResponse, Timeframe timeframe)
    {
        var results = new float[timeframe.Multiplier];

        for(int i = 0; i < results.Length; i++)
        {
            results[i] = Value;
        }

        return results;
    }

    public bool HasTimespan(out Timespan? timespan)
    {
        timespan = null;
        return false;
    }
}
