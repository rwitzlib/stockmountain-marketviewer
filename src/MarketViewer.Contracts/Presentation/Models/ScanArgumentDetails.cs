using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Models;

[ExcludeFromCodeCoverage]
public class ScanArgumentDetails
{
    public string Operator { get; set; }
    public List<FilterDetails> Filters { get; set; }
    public ScanArgumentDetails Argument { get; set; }
}
