using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Models.ScanV2;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
[JsonConverter(typeof(ScanArgumentConverter))]
public class ScanArgument
{
    public string Operator { get; set; }
    public List<FilterV2> Filters { get; set; }
    public ScanArgument Argument { get; set; }
}
