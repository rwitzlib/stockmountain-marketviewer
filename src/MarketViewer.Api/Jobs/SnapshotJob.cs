using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using Polygon.Client.Interfaces;
using Polygon.Client.Models;
using Quartz;
using System.Diagnostics;

namespace MarketViewer.Api.Jobs;

public class SnapshotJob(
    MarketCache _marketCache,
    IPolygonClient _polygonClient,
    ILogger<SnapshotJob> _logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var jobDataMap = context.JobDetail.JobDataMap;
        if (!jobDataMap.TryGetString("timespan", out var timespanString))
        {
            _logger.LogInformation("Missing required timespan parameter.");
            return;
        }

        if (!Enum.TryParse<Timespan>(timespanString, out var timespan))
        {
            _logger.LogInformation("Invalid timespan.");
        }

        if (DateTimeOffset.Now.DayOfWeek == DayOfWeek.Saturday || DateTimeOffset.Now.DayOfWeek == DayOfWeek.Sunday)
        {
            _logger.LogInformation("Market Closed - Skipping snapshot job.");
            return;
        }

        var sp = new Stopwatch();
        sp.Start();
        _logger.LogInformation("Snapshot({timespan}) - Started at: {time}.", timespan, DateTimeOffset.Now);

        var snapshotResponse = await _polygonClient.GetAllTickersSnapshot(null);

        var tasks = new List<Task>();
        foreach (var snapshot in snapshotResponse.Tickers)
        {
            tasks.Add(Task.Run(() => AddBarToCache(snapshot.Ticker, timespan, snapshot.Minute)));
        }
        await Task.WhenAll(tasks);
        sp.Stop();

        _logger.LogInformation("Snapshot({timespan}) - Finished at: {time}.", timespan, DateTimeOffset.Now);
        _logger.LogInformation("Snapshot({timespan}) - Time elapsed: {elapsed}ms.", timespan, sp.ElapsedMilliseconds);
    }

    private Task AddBarToCache(string ticker, Timespan timespan, Bar bar)
    {
        if (bar.Timestamp == 0)
        {
            return Task.CompletedTask;
        }

        var stocksResponse = _marketCache.GetStocksResponse(ticker, timespan, DateTimeOffset.Now);

        if (stocksResponse is not null && stocksResponse.Results.Any())
        {
            var last = stocksResponse.Results.Last();
            if (last.Timestamp < bar.Timestamp)
            {
                stocksResponse.Results.Add(bar);

                _marketCache.SetStocksResponse(stocksResponse, timespan, DateTimeOffset.Now);
            }
            else if (last.Timestamp == bar.Timestamp)
            {
                
            }
            else
            {

            }
        }

        return Task.CompletedTask;
    }
}
