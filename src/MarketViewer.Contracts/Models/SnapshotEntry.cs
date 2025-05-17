using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models;

[ExcludeFromCodeCoverage]
public class SnapshotEntry
{
    public string Ticker { get; set; }
    public List<Snapshot> Results { get; set; }
}
