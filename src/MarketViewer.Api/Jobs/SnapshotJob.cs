using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using Polygon.Client.Interfaces;
using Polygon.Client.Models;
using Quartz;
using System.Diagnostics;

namespace MarketViewer.Api.Jobs;

public class SnapshotJob(
    IMarketCache marketCache,
    IPolygonClient polygonClient,
    ILogger<SnapshotJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var sp = new Stopwatch();
        sp.Start();

        var timespan = Enum.Parse<Timespan>(context.JobDetail.JobDataMap.GetString("timespan"));

        logger.LogInformation("Snapshot({timespan}) - Started at: {time}.", timespan, DateTimeOffset.Now);

        if (DateTimeOffset.Now.DayOfWeek == DayOfWeek.Saturday || DateTimeOffset.Now.DayOfWeek == DayOfWeek.Sunday)
        {
            logger.LogInformation("Snapshot({timespan}) - Market Closed - Skipping snapshot job.", timespan);
            return;
        }

        try
        {
            var snapshotResponse = await polygonClient.GetAllTickersSnapshot(null);

            var tasks = new List<Task>();
            foreach (var snapshot in snapshotResponse.Tickers)
            {
                tasks.Add(Task.Run(() => AddBarToCache(snapshot.Ticker, timespan, snapshot.Minute)));
            }
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError("Snapshot({timespan}) - Error populating snapshot data: {message}", timespan, ex.Message);
        }
        finally
        {
            sp.Stop();

            logger.LogInformation("Snapshot({timespan}) - Finished at: {time}. Time elapsed: {elapsed}ms.", timespan, DateTimeOffset.Now, sp.ElapsedMilliseconds);
        }
    }

    private Task AddBarToCache(string ticker, Timespan timespan, Bar bar)
    {
        if (bar.Timestamp == 0)
        {
            return Task.CompletedTask;
        }

        var stocksResponse = marketCache.GetStocksResponse(ticker, timespan, DateTimeOffset.Now);

        if (stocksResponse is not null && stocksResponse.Results.Any())
        {
            var last = stocksResponse.Results.Last();
            if (last.Timestamp < bar.Timestamp)
            {
                stocksResponse.Results.Add(bar);

                marketCache.SetStocksResponse(stocksResponse, timespan, DateTimeOffset.Now);
            }
            else if (last.Timestamp > bar.Timestamp)
            {
                logger.LogWarning("Snapshot timestamp for {ticker} was greater than last timestamp.", stocksResponse.Ticker);
            }
        }

        return Task.CompletedTask;
    }
}
