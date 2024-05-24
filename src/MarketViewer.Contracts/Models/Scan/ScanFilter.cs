using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class ScanFilter
{
    public IScanOperand First { get; set; }
    public string Operator { get; set; }
    public IScanOperand Second { get; set; }
}
