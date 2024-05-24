using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class ScanArgument
{
    public string Operator { get; set; }
    public ScanFilter[] Filters { get; set; }
    public ScanArgument Arguments { get; set; }
}
