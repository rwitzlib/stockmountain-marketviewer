using MarketViewer.Contracts.Responses;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Interfaces;
using Polygon.Client.Models;
using Quartz;
using System.Diagnostics;

namespace MarketViewer.Api.Jobs
{
    public class SnapshotJob(
        IPolygonClient polygonClient,
        IMemoryCache memoryCache,
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
            var aggregate = memoryCache.Get<StocksResponse>($"Stocks/{ticker}/minute/{DateTime.Now:yyyyMMdd}");

            aggregate?.Results.Add(bar);

            memoryCache.Set($"Stock_{ticker}_{DateTime.Now:yyyyMMdd}", aggregate);

            return Task.CompletedTask;
        }
    }
}
