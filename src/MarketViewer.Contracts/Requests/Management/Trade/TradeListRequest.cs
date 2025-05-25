using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Management.Trade;

[ExcludeFromCodeCoverage]
public class TradeListRequest
{
    public string Strategy { get; set; }
    public string User { get; set; }
}
