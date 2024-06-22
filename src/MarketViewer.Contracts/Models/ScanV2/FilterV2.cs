using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.ScanV2;

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
