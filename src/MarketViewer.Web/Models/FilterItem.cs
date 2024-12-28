using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.ScanV2.Operands;
using MarketViewer.Web.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Models;

[ExcludeFromCodeCoverage]
public class FilterItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CollectionModifier { get; set; }
    public OperandType FirstOperandType { get; set; }
    public IScanOperand FirstOperand { get; set; }
    public FilterOperator Operator { get; set; }
    public OperandType SecondOperandType { get; set; }
    public IScanOperand SecondOperand { get; set; }
    public Timeframe Timeframe { get; set; } = new();
}
