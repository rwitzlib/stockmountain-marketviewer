using AutoMapper;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using Polygon.Client.Requests;
using Quartz;
using System.Diagnostics;
using Polygon.Client.Interfaces;
using Polygon.Client.Models;

namespace MarketViewer.Api.Jobs;

public class UpdateAggregateJob(
    IMarketCache marketCache,
    IPolygonClient polygonClient,
    IMapper mapper,
    ILogger<UpdateAggregateJob> logger) : IJob
{
    private readonly Stopwatch _sp = new();

    private readonly int BATCH_SIZE = int.TryParse(Environment.GetEnvironmentVariable("BATCH_SIZE"), out int size) ? size : 12000;

    public async Task Execute(IJobExecutionContext context)
    {
        _sp.Start();
        var timespan = Enum.Parse<Timespan>(context.JobDetail.JobDataMap.GetString("timespan"));

        try
        {
            logger.LogInformation("Started populating {timespan} aggregate data at: {time}.", timespan, DateTimeOffset.Now);

            switch (timespan)
            {
                case Timespan.minute:
                    var snapshotResponse = await polygonClient.GetAllTickersSnapshot(null);

                    var tasks = new List<Task>();
                    foreach (var snapshot in snapshotResponse.Tickers)
                    {
                        tasks.Add(Task.Run(() => AddBarToCache(snapshot.Ticker, timespan, snapshot.Minute)));
                    }
                    await Task.WhenAll(tasks); 
                    
                    foreach (var snapshot in snapshotResponse.Tickers)
                    {
                        AddBarToCache(snapshot.Ticker, timespan, snapshot.Minute);
                    }
                    
                    break;

                case Timespan.hour:
                    await PopulateStocksResponses(timespan, DateTimeOffset.Now);
                    break;
            }

            _sp.Stop();
            logger.LogInformation("Aggregate({timespan}) - Finished populating at: {time}. Time elapsed: {elapsed}ms.", timespan, DateTimeOffset.Now, _sp.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError("UpdateAggregate({timespan}) - Error populating aggregate data: {message}", timespan, ex.Message);
        }
    }

    private async Task PopulateStocksResponses(Timespan timespan, DateTimeOffset date)
    {
        var tickers = marketCache.GetTickers();

        for (int i = 0; i < tickers.Count(); i += BATCH_SIZE)
        {
            var tasks = new List<Task<StocksResponse>>();

            var batch = tickers.Take(new Range(i, i + BATCH_SIZE));

            foreach (var ticker in batch)
            {
                tasks.Add(Task.Run(() => PopulateStocksResponse(ticker, 1, timespan, date)));
            }
            var stocksResponses = await Task.WhenAll(tasks);

            foreach (var stocksResponse in stocksResponses)
            {
                AddBarToCache(stocksResponse.Ticker, timespan, stocksResponse.Results.First());
            }
        }
    }

    private async Task<StocksResponse> PopulateStocksResponse(string ticker, int multiplier, Timespan timespan, DateTimeOffset date)
    {
        var polygonAggregateRequest = new PolygonAggregateRequest
        {
            Ticker = ticker,
            Multiplier = multiplier,
            Timespan = timespan.ToString(),
            From = DateTime.Now.ToString("yyyy-MM-dd"),
            To = DateTime.Now.ToString("yyyy-MM-dd"),
            Limit = 1,
            Sort = "desc"
        };
        var polygonAggregateResponse = await polygonClient.GetAggregates(polygonAggregateRequest);

        var stocksResponse = mapper.Map<StocksResponse>(polygonAggregateResponse);

        return stocksResponse;
    }

    public void AddBarToCache(string ticker, Timespan timespan, Bar bar)
    {
        if (bar.Timestamp == 0)
        {
            return;
        }

        var stocksResponse = marketCache.GetStocksResponse(ticker, timespan, DateTimeOffset.Now);

        if (stocksResponse is not null && stocksResponse.Results.Any())
        {
            var last = stocksResponse.Results.Last();
            if (last.Timestamp < bar.Timestamp)
            {
                if (stocksResponse.Ticker is "MARA")
                {

                }
                var ticks = TimeSpan.FromMinutes(1).Ticks / 10000;
                if (last.Timestamp + ticks == bar.Timestamp)
                {

                }
                else
                {

                }
                stocksResponse.Results.Add(bar);

                marketCache.SetStocksResponse(stocksResponse, timespan, DateTimeOffset.Now);
            }
        }

        return;
    }
}
