using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;
using MarketViewer.Web.Components;

namespace MarketViewer.Web.Models
{
    [ExcludeFromCodeCoverage]
    public class ScannerDisplayItem
    {
        public string Id { get; set; }
        public ScanResponse.Item Item { get; set; }
        public bool Displayed { get; set; }
        public bool Decayed { get; set; } = false;
        public StocksRequest Request { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public ChartComponent ChartComponent { get; set; }
    }
}
