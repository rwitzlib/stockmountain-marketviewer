using AutoMapper;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using Polygon.Client.Requests;
using Quartz;
using System.Diagnostics;
using Polygon.Client.Interfaces;

namespace MarketViewer.Api.Jobs;

public class InitialAggregateJob(
    IMarketCache marketCache,
    IPolygonClient polygonClient,
    IMapper mapper,
    ILogger<InitialAggregateJob> logger) : IJob
{
    private readonly Stopwatch _sp = new();

    public async Task Execute(IJobExecutionContext context)
    {
        var timespan = Enum.Parse<Timespan>(context.JobDetail.JobDataMap.GetString("timespan"));
        _sp.Start();

        logger.LogInformation("Initializing {timespan} aggregate data at: {time}.", timespan, DateTimeOffset.Now);

        try
        {
            var tickers = marketCache.GetTickers();

            var batchSize = int.TryParse(Environment.GetEnvironmentVariable("BATCH_SIZE"), out int size) ? size : 12000;

            await PopulateStocksResponses(tickers, batchSize, timespan, DateTimeOffset.Now);

            _sp.Stop();

            logger.LogInformation("InitialAggregate({timespan}) - Finished populating at: {time}. Time elapsed: {elapsed}ms.", timespan, DateTimeOffset.Now, _sp.ElapsedMilliseconds);

            var now = DateTimeOffset.Now;

            var interval = timespan switch
            {
                Timespan.minute => 1,
                Timespan.hour => 60,
                _ => 0
            };

            var startTime = timespan switch
            {
                Timespan.minute => new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 1, 0, now.Offset),
                // Start at 9:01, 10:01, etc. to get the minute before: 9:00, 10:00, etc.
                Timespan.hour => new DateTimeOffset(now.Year, now.Month, now.Day, now.AddHours(1).Hour, 1, 1, 0, now.Offset),
                _ => new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 1, 0, now.Offset)
            };

            var repeatCount = timespan switch
            {
                Timespan.minute => 1080, // 18 Hours
                Timespan.hour => 18,
                _ => throw new NotImplementedException()
            };

            var updateJob = JobBuilder.Create<UpdateAggregateJob>()
                .WithIdentity($"UpdateAggregate-{timespan}-{Guid.NewGuid()}")
                .UsingJobData("timespan", timespan.ToString())
                .StoreDurably(true)
                .Build();

            var updateTrigger = TriggerBuilder.Create()
                .WithIdentity($"UpdateAggregateTrigger-{timespan}-{Guid.NewGuid()}")
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInMinutes(interval)
                    .WithRepeatCount(repeatCount))
                .ForJob(updateJob)
                .StartAt(startTime)
                .Build();

            await context.Scheduler.ScheduleJob(updateJob, updateTrigger);
        }
        catch (Exception ex)
        {
            logger.LogError("InitialAggregate({timespan}) - Error populating aggregate data: {message}", timespan, ex.Message);
        }
    }

    private async Task PopulateStocksResponses(IEnumerable<string> tickers, int batchSize, Timespan timespan, DateTimeOffset date)
    {
        for (int i = 0; i < tickers.Count(); i += batchSize)
        {
            var tasks = new List<Task>();

            var batch = tickers.Take(new Range(i, i + batchSize));

            foreach (var ticker in batch)
            {
                tasks.Add(Task.Run(() => PopulateStocksResponse(ticker, 1, timespan, date)));
            }
            await Task.WhenAll(tasks);
        }
    }

    private async Task PopulateStocksResponse(string ticker, int multiplier, Timespan timespan, DateTimeOffset date)
    {
        var start = date.Add(GetStartOffset(timespan));

        var polygonAggregateRequest = new PolygonAggregateRequest
        {
            Ticker = ticker,
            Multiplier = multiplier,
            Timespan = timespan.ToString(),
            From = start.ToString("yyyy-MM-dd"),
            To = DateTime.Now.ToString("yyyy-MM-dd"),
            Limit = 50000
        };
        var polygonAggregateResponse = await polygonClient.GetAggregates(polygonAggregateRequest);

        var stocksResponse = mapper.Map<StocksResponse>(polygonAggregateResponse);

        marketCache.SetStocksResponse(stocksResponse, timespan, date);
    }

    private static TimeSpan GetStartOffset(Timespan timespan)
    {
        return timespan switch
        {
            Timespan.minute => TimeSpan.FromDays(-5),
            Timespan.hour => TimeSpan.FromDays(-60),
            Timespan.day => throw new NotImplementedException(),
            Timespan.week => throw new NotImplementedException(),
            Timespan.month => throw new NotImplementedException(),
            Timespan.quarter => throw new NotImplementedException(),
            Timespan.year => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }
}
