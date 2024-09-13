using Polygon.Client.Interfaces;
using Quartz;
using AutoMapper;
using Polygon.Client.Requests;
using System.Diagnostics;
using MarketViewer.Contracts.Responses;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Caching;

namespace MarketViewer.Api.Jobs;

public class StocksHourJob(
    MarketCache _marketCache,
    IPolygonClient polygonClient,
    IMapper mapper,
    ILogger<StocksHourJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        if (DateTimeOffset.Now.DayOfWeek == DayOfWeek.Saturday || DateTimeOffset.Now.DayOfWeek == DayOfWeek.Sunday)
        {
            logger.LogInformation("Market Closed - Skipping stocks job.");
            return;
        }

        logger.LogInformation("Stocks - Started populating at: {time}.", DateTimeOffset.Now);

        var sp = new Stopwatch();
        sp.Start();


        var tickers = _marketCache.GetTickersByTimespan(Timespan.hour, DateTimeOffset.Now);

        var tasks = new List<Task>();
        foreach (var ticker in tickers)
        {
            tasks.Add(Task.Run(() => PopulateStocksResponse(ticker, 1, Timespan.hour)));
        }
        await Task.WhenAll(tasks);

        sp.Stop();

        logger.LogInformation("Stocks - Finished populating at: {time}.", DateTimeOffset.Now);
        logger.LogInformation("Stocks - Time elapsed: {elapsed}ms.", sp.ElapsedMilliseconds);
    }

    private async Task PopulateStocksResponse(string ticker, int multiplier, Timespan timespan)
    {
        var polygonAggregateRequest = new PolygonAggregateRequest
        {
            Ticker = ticker,
            Multiplier = multiplier,
            Timespan = timespan.ToString(),
            From = ((DateTimeOffset)DateTime.Now.AddMonths(-2).Date).ToUnixTimeMilliseconds().ToString(),
            To = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString(),
            Limit = 50000
        };
        var polygonAggregateResponse = await polygonClient.GetAggregates(polygonAggregateRequest);

        var stocksResponse = mapper.Map<StocksResponse>(polygonAggregateResponse);

        var asdf = _marketCache.GetStocksResponse(ticker, timespan, DateTimeOffset.Now);

        _marketCache.SetStocksResponse(stocksResponse, timespan, DateTimeOffset.Now);
    }
}
