using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests;

[ExcludeFromCodeCoverage]
public class ScannerRequest
{
    public ScanArgument Arguments { get; set; }
    public string Timestamp { get; set; }
}
