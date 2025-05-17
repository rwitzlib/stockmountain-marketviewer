using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Dtos;

[ExcludeFromCodeCoverage]
public class ScanArgumentDto
{
    public string Operator { get; set; }
    public List<FilterDto> Filters { get; set; }
    public ScanArgumentDto Argument { get; set; }
}
