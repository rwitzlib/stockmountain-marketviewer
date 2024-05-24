using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class ValueOperand : IScanOperand
{
    public float Value { get; set; }
}
