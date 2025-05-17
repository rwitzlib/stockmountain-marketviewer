using MarketViewer.Contracts.Models;
using System.Diagnostics.CodeAnalysis;


namespace MarketViewer.Contracts.Responses;

[ExcludeFromCodeCoverage]
public class SnapshotResponse
{
    public List<SnapshotEntry> Entries { get; set; }
}
