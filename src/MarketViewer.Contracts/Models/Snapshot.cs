using Polygon.Client.Models;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models;

[ExcludeFromCodeCoverage]
public class Snapshot
{
    public long Timestamp { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public List<Bar> Minute { get; set; }
    public List<Bar> Hour { get; set; }
}
