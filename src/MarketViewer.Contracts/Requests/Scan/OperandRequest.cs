using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Scan;

[ExcludeFromCodeCoverage]
public class OperandRequest 
{
    public OperandType Type { get; set; }
    public string Name { get; set; }
    public string Parameters { get; set; }
    public OperandModifier? Modifier { get; set; }
    public Timeframe Timeframe { get; set; }
    public float? Value { get; set; }
}
