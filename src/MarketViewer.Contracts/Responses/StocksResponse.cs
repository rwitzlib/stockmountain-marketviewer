using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Models.Study;
using Polygon.Client.Models;

namespace MarketViewer.Contracts.Responses;

[ExcludeFromCodeCoverage]
public class StocksResponse
{
    /// <summary>
    /// The exchange symbol that this item is traded under.
    /// </summary>
    public string Ticker { get; set; }

    /// <summary>
    /// The status of this request's response.
    /// </summary>
    public string Status { get; set; }

    public List<Bar> Results { get; set; }

    public List<StudyResponse> Studies { get; set; }
    public Information TickerInfo { get; set; } = new();

    public class Information
    {
        public TickerDetails TickerDetails { get; set; } = new();
        public float DailyVolume { get; set; }
        public float AverageVolume { get; set; }
    }
}
