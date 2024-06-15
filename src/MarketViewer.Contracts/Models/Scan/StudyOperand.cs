using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class StudyOperand : IScanOperand
{
    [JsonRequired]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StudyType Type { get; set; }

    [JsonRequired]
    public string[] Parameters { get; set; }

    [JsonRequired]
    public int Multiplier { get; set; }

    [JsonRequired]
    [JsonConverter(typeof(JsonStringEnumConverter))]
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
