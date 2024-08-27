using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Infrastructure.Services;
using Polygon.Client.Interfaces;
using Polygon.Client.Models;
using Quartz;
using System.Diagnostics;

namespace MarketViewer.Api.Jobs;

public class SnapshotJob(
    MarketCache _marketCache,
    IPolygonClient polygonClient,
    ILogger<SnapshotJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        if (DateTimeOffset.Now.DayOfWeek == DayOfWeek.Saturday || DateTimeOffset.Now.DayOfWeek == DayOfWeek.Sunday)
        {
            logger.LogInformation("Market Closed - Skipping snapshot job.");
            return;
        }

        var sp = new Stopwatch();
        sp.Start();
        logger.LogInformation("Snapshot - Started at: {time}.", DateTimeOffset.Now);

        var snapshotResponse = await polygonClient.GetAllTickersSnapshot(null);

        var tasks = new List<Task>();
        foreach (var snapshot in snapshotResponse.Tickers)
        {
            tasks.Add(Task.Run(() => AddBarToCache(snapshot.Ticker, snapshot.Minute)));
        }
        await Task.WhenAll(tasks);
        sp.Stop();

        logger.LogInformation("Snapshot - Finished at: {time}.", DateTimeOffset.Now);
        logger.LogInformation("Snapshot - Time elapsed: {elapsed}ms.", sp.ElapsedMilliseconds);
    }

    private Task AddBarToCache(string ticker, Bar bar)
    {
        var stocksResponse = _marketCache.GetStocksResponse(ticker, Timespan.minute, DateTimeOffset.Now);

        stocksResponse?.Results.Add(bar);

        _marketCache.SetStocksResponse(stocksResponse, Timespan.minute, DateTimeOffset.Now);

        return Task.CompletedTask;
    }
}
