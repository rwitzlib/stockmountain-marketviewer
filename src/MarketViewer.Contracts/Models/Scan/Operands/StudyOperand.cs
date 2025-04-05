using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan.Operands;

[ExcludeFromCodeCoverage]
public class StudyOperand : IScanOperand
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StudyType Study { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OperandModifier Modifier { get; set; }

    public string Parameters { get; set; }

    public Timeframe Timeframe { get; set; }

    public OperandType GetOperandType()
    {
        return OperandType.Study;
    }

    public int GetPriority()
    {
        return 10;
    }

    public bool HasTimeframe(out Timeframe timeframe)
    {
        timeframe = Timeframe;
        return true;
    }
}
