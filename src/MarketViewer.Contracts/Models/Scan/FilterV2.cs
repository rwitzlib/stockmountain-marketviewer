using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.ScanV2.Operands;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
[JsonConverter(typeof(FilterConverter))]
public class FilterV2
{
    public string CollectionModifier { get; set; }
    public IScanOperand FirstOperand { get; set; }
    public FilterOperator Operator { get; set; }
    public IScanOperand SecondOperand { get; set; }
    public Timeframe Timeframe { get; set; }
}
