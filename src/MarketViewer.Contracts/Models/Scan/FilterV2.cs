using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class FilterV2
{
    public string CollectionModifier { get; set; }
    public IScanOperand FirstOperand { get; set; }
    public FilterOperator Operator { get; set; }
    public IScanOperand SecondOperand { get; set; }
    public Timeframe Timeframe { get; set; }
}

public class Timeframe
{
    public int Multiplier { get; set; }
    public Timespan Timespan { get; set; }
}
