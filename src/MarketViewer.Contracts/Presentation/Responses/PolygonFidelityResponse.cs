using MarketViewer.Contracts.Entities;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Responses;

[ExcludeFromCodeCoverage]
public class PolygonFidelityResponse
{
    public PolygonFidelity Minute { get; set; }
    public PolygonFidelity Hour { get; set; }
}
