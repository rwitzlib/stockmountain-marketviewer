using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Scan;

[ExcludeFromCodeCoverage]
public class ScanArgumentRequest
{
    public string Operator { get; set; }
    public List<FilterRequest> Filters { get; set; }
    public ScanArgumentRequest Argument { get; set; }
}
