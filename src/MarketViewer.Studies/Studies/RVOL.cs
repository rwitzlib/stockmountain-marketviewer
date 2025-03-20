using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Responses;
using Polygon.Client.Interfaces;

namespace MarketViewer.Studies.Studies;

public class RVOL(IMarketCache marketCache, IPolygonClient polygonClient) : IStudy
{
    public List<List<LineEntry>> Compute(string[] parameters, ref StocksResponse stocksResponse)
    {
        var results = new List<List<LineEntry>>
        {
            new()
        };

        if (parameters.Length > 0)
        {
            return results;
        }

        var dailyResponse = marketCache.GetStocksResponse(stocksResponse.Ticker, new Timeframe(1, Timespan.day), DateTimeOffset.Now);

        return results;
    }
}