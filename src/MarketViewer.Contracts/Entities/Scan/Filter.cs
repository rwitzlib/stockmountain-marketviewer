using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan.Operands;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

[JsonConverter(typeof(FilterConverter))]
[ExcludeFromCodeCoverage]
public class Filter
{
    public string CollectionModifier { get; set; }
    public IScanOperand FirstOperand { get; set; }
    public FilterOperator Operator { get; set; }
    public IScanOperand SecondOperand { get; set; }
    public Timeframe Timeframe { get; set; }
}
