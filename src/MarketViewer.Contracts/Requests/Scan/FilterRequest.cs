using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Scan;

[ExcludeFromCodeCoverage]
public class FilterRequest
{
    public string CollectionModifier { get; set; }
    public OperandRequest FirstOperand { get; set; }
    public FilterOperator Operator { get; set; }
    public OperandRequest SecondOperand { get; set; }
    public Timeframe Timeframe { get; set; }
}
