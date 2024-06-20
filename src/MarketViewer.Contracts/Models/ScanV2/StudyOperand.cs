using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.ScanV2;

[ExcludeFromCodeCoverage]
public class StudyOperand : IScanOperand
{
    [JsonRequired]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Modifier ValueType { get; set; }

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

    public bool HasTimespan(out Timespan? timespan)
    {
        timespan = Timespan;
        return true;
    }
}
