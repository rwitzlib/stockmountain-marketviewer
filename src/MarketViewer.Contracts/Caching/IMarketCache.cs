using MarketViewer.Contracts.Responses;
using Polygon.Client.Models;
using MarketViewer.Contracts.Enums;

namespace MarketViewer.Contracts.Caching;

public interface IMarketCache
{
    Task<IEnumerable<StocksResponse>> Initialize(DateTimeOffset date, int multiplier, Timespan timespan);

    IEnumerable<string> GetTickers();
    void SetTickers(IEnumerable<string> tickers);

    IEnumerable<string> GetTickersByTimespan(Timespan timespan, DateTimeOffset timestamp);
    void SetTickersByTimespan(DateTimeOffset date, Timespan timespan, IEnumerable<string> tickers);

    StocksResponse GetStocksResponse(string ticker, Timespan timespan, DateTimeOffset timestamp);
    void SetStocksResponse(StocksResponse stocksResponse, Timespan timespan, DateTimeOffset date);

    TickerDetails GetTickerDetails(string ticker);
    void SetTickerDetails(TickerDetails tickerDetails);
}
