using MarketViewer.Core.ScanV2;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class ScanArgument
{
    public string Operator { get; set; }
    public FilterV2[] Filters { get; set; }
    public ScanArgument Argument { get; set; }
}
