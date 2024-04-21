using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models;

[ExcludeFromCodeCoverage]
public class LineEntry
{
    public long Timestamp { get; set; }
    public float Value { get; set; }
}