using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.ScanV2;

[ExcludeFromCodeCoverage]
public class StocksResponseCollection
{
    public Dictionary<Timespan, IEnumerable<StocksResponse>> Responses { get; set; } = [];

    public StocksResponseCollection FilterByTicker(IEnumerable<string> tickers)
    {
        if (tickers is null || !tickers.Any())
        {
            return this;
        }

        var filteredResponseCollection = new Dictionary<Timespan, IEnumerable<StocksResponse>>();
        foreach (var response in Responses)
        {
            var filteredResponses = response.Value.Where(q => tickers.Contains(q.Ticker));
            filteredResponseCollection.Add(response.Key, filteredResponses);
        }

        return new StocksResponseCollection
        {
            Responses = filteredResponseCollection
        };
    }
}
