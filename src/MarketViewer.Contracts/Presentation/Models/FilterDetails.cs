using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Models;

[ExcludeFromCodeCoverage]
public class FilterDetails
{
    public string CollectionModifier { get; set; }
    public OperandDetails FirstOperand { get; set; }
    public FilterOperator Operator { get; set; }
    public OperandDetails SecondOperand { get; set; }
    public Timeframe Timeframe { get; set; }
}
