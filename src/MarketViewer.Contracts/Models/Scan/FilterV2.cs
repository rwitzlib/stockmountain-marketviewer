using MarketViewer.Application.Utilities;
using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class FilterV2
{
    public string CollectionModifier { get; set; }

    [JsonConverter(typeof(ScanOperandConverter))]
    public IScanOperand FirstOperand { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public FilterOperator Operator { get; set; }

    [JsonConverter(typeof(ScanOperandConverter))]
    public IScanOperand SecondOperand { get; set; }
    public Timeframe Timeframe { get; set; }
}

public class Timeframe
{
    public int Multiplier { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Timespan Timespan { get; set; }
}
