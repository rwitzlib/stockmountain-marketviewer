using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class ValueOperand : IScanOperand
{
    [JsonRequired]
    public float Value { get; set; }

    public float[] Compute(StocksResponse stocksResponse, Timeframe timeframe)
    {
        return [ Value ];
    }

    public bool HasTimespan(out Timespan? timespan)
    {
        timespan = null;
        return false;
    }
}
