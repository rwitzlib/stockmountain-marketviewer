using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Management.Trade;

[ExcludeFromCodeCoverage]
public class TradeCloseRequest
{
    public string ClosedAt { get; set; }
    public float ClosePrice { get; set; }
    public float ClosePosition { get; set; }
    public float Profit { get; set; }
}
