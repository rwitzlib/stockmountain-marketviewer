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
    private readonly int BATCH_SIZE = int.TryParse(Environment.GetEnvironmentVariable("BATCH_SIZE"), out int size) ? size : 12000;

    public async Task Execute(IJobExecutionContext context)
    {
        var sp = new Stopwatch();
        sp.Start();

        logger.LogInformation("Initializing aggregate data at: {time}.", DateTimeOffset.Now);

        try
        {
            await PopulateStocksResponses(Timespan.minute, DateTimeOffset.Now);
            logger.LogInformation("Finished initializing minute aggregate data at: {time}. Time elapsed: {elapsed}ms.", DateTimeOffset.Now, sp.ElapsedMilliseconds);

            await PopulateStocksResponses(Timespan.hour, DateTimeOffset.Now);
            logger.LogInformation("Finished initializing hourly aggregate data at: {time}. Time elapsed: {elapsed}ms.", DateTimeOffset.Now, sp.ElapsedMilliseconds);

            sp.Stop();

            if (sp.Elapsed.TotalSeconds > 60)
            {
                logger.LogInformation("Initializing aggregate data took longer than 1 minute");

                var singleSnapshotJob = JobBuilder.Create<SnapshotJob>()
                    .StoreDurably(true)
                    .Build();

                var singleSnapshotTrigger = TriggerBuilder.Create()
                    .ForJob(singleSnapshotJob)
                    .StartAt(DateTimeOffset.Now)
                    .Build();

                await context.Scheduler.ScheduleJob(singleSnapshotJob, singleSnapshotTrigger);
            }

            var now = DateTimeOffset.Now;
            var startTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 1, 0, now.Offset);

            var scheduledSnapshotJob = JobBuilder.Create<SnapshotJob>()
                .StoreDurably(true)
                .Build();

            var scheduledSnapshotTrigger = TriggerBuilder.Create()
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInMinutes(1)
                    .WithRepeatCount(1800)) // 18 hours
                .ForJob(scheduledSnapshotJob)
                .StartAt(startTime)
                .Build();

            await context.Scheduler.ScheduleJob(scheduledSnapshotJob, scheduledSnapshotTrigger);
        }
        catch (Exception ex)
        {
            logger.LogError("Error initializing aggregate data: {message}", ex.Message);
        }
    }

    private async Task PopulateStocksResponses(Timespan timespan, DateTimeOffset date)
    {
        var tickers = marketCache.GetTickers();

        for (int i = 0; i < tickers.Count(); i += BATCH_SIZE)
        {
            var tasks = new List<Task>();

            var batch = tickers.Take(new Range(i, i + BATCH_SIZE));

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
            Timespan.hour => TimeSpan.FromDays(-30),
            Timespan.day => throw new NotImplementedException(),
            Timespan.week => throw new NotImplementedException(),
            Timespan.month => throw new NotImplementedException(),
            Timespan.quarter => throw new NotImplementedException(),
            Timespan.year => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }
}
