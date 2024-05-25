using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class FilterV2
{
    public IScanOperand First { get; set; }
    public string Operator { get; set; }
    public IScanOperand Second { get; set; }
    public Timeframe Timeframe { get; set; }
}

public class Timeframe
{
    public int Multiplier { get; set; }
    public Timespan Timespan { get; set; }
}
