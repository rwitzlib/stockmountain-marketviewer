using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Interfaces;
using Quartz;
using AutoMapper;
using Polygon.Client.Requests;
using System.Diagnostics;
using MarketViewer.Contracts.Responses;

namespace MarketDataProvider.Api.Jobs
{
    public class StocksJob(
        IPolygonClient polygonClient,
        IMemoryCache memoryCache,
        IMapper mapper,
        ILogger<StocksJob> logger) : IJob
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

            var tickers = memoryCache.Get<IEnumerable<string>>("Tickers");

            var tasks = new List<Task>();
            foreach (var ticker in tickers)
            {
                tasks.Add(Task.Run(() => PopulateStocksResponse(ticker)));
            }
            await Task.WhenAll(tasks);

            sp.Stop();

            logger.LogInformation("Stocks - Finished populating at: {time}.", DateTimeOffset.Now);
            logger.LogInformation("Stocks - Time elapsed: {elapsed}ms.", sp.ElapsedMilliseconds);
        }

        private async Task PopulateStocksResponse(string ticker)
        {
            var polygonAggregateRequest = new PolygonAggregateRequest
            {
                Ticker = ticker,
                Multiplier = 1,
                Timespan = "minute",
                From = ((DateTimeOffset)DateTime.Now.Date).ToUnixTimeMilliseconds().ToString(),
                To = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()
            };
            var polygonAggregateResponse = await polygonClient.GetAggregates(polygonAggregateRequest);

            var stocksResponse = mapper.Map<StocksResponse>(polygonAggregateResponse);

            memoryCache.Set($"Stocks/{ticker}/minute/{DateTime.Now:yyyyMMdd}", stocksResponse);
        }
    }
}
