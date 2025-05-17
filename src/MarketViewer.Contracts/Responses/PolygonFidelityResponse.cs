using MarketViewer.Contracts.Models;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Responses;

[ExcludeFromCodeCoverage]
public class PolygonFidelityResponse
{
    public PolygonFidelity Minute { get; set; }
    public PolygonFidelity Hour { get; set; }
}
