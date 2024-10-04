using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.ScanV2.Operands;

[ExcludeFromCodeCoverage]
public class StudyOperand : IScanOperand
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StudyType Study { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OperandModifier Modifier { get; set; }

    public string Parameters { get; set; }

    public int Multiplier { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Timespan Timespan { get; set; }

    public int GetPriority()
    {
        return 10;
    }

    public bool HasTimeframe(out int? multiplier, out Timespan? timespan)
    {
        multiplier = Multiplier;
        timespan = Timespan;
        return true;
    }
}
