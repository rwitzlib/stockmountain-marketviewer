﻿using AutoMapper;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using Polygon.Client.Requests;
using Quartz;
using System.Diagnostics;
using Polygon.Client.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Snapshot;
using static System.Runtime.InteropServices.JavaScript.JSType;
using MarketViewer.Contracts.Responses.Tools;
using MarketViewer.Contracts.Responses.Market;

namespace MarketViewer.Api.Jobs;

public class InitialAggregateJob(
    IMemoryCache memoryCache,
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
            var timeframes = new List<Timeframe>
            {
                new (1, Timespan.minute),
                new (1, Timespan.hour),
                new (1, Timespan.day)
            };

            foreach (var timeframe in timeframes)
            {
                await PopulateStocksResponses(timeframe, DateTimeOffset.Now);
                logger.LogInformation("Finished initializing {timespan} aggregate data at: {time}. Time elapsed: {elapsed}ms.", timeframe.Timespan, DateTimeOffset.Now, sp.ElapsedMilliseconds);
            }

            SetSnapshot();

            if (sp.Elapsed.TotalSeconds > 60 && Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") is not "local")
            {
                logger.LogInformation("Initializing aggregate data took longer than 1 minute. Starting over.");

                var schedulerJob = JobBuilder.Create<SchedulerJob>()
                    .Build();

                var schedulerTrigger = TriggerBuilder.Create()
                    .ForJob(schedulerJob)
                    .StartAt(DateTimeOffset.Now)
                    .Build();

                await context.Scheduler.ScheduleJob(schedulerJob, schedulerTrigger);

                sp.Stop();

                return;
            }
            sp.Stop();

            var now = DateTimeOffset.Now;
            var startTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 1, 0, now.Offset);

            var scheduledSnapshotJob = JobBuilder.Create<SnapshotJob>()
                .StoreDurably(true)
                .Build();

            var scheduledSnapshotTrigger = TriggerBuilder.Create()
                .WithSimpleSchedule(schedule => schedule
                    .WithIntervalInMinutes(1)
                    .WithRepeatCount(540)) // 9 hours
                .ForJob(scheduledSnapshotJob)
                .StartAt(startTime)
                .Build();

            var snapshot = memoryCache.Get<SnapshotResponse>("snapshot");

            await context.Scheduler.ScheduleJob(scheduledSnapshotJob, scheduledSnapshotTrigger);
        }
        catch (Exception ex)
        {
            logger.LogError("Error initializing aggregate data: {message}", ex.Message);
        }
    }

    private async Task PopulateStocksResponses(Timeframe timeframe, DateTimeOffset date)
    {
        var tickers = marketCache.GetTickers();

        for (int i = 0; i < tickers.Count(); i += BATCH_SIZE)
        {
            var tasks = new List<Task>();

            var batch = tickers.Take(new Range(i, i + BATCH_SIZE));

            foreach (var ticker in batch)
            {
                tasks.Add(Task.Run(() => PopulateStocksResponse(ticker, timeframe, date)));
            }
            await Task.WhenAll(tasks);
        }
    }

    private async Task PopulateStocksResponse(string ticker, Timeframe timeframe, DateTimeOffset date)
    {
        var start = date.Add(GetStartOffset(timeframe.Timespan));

        var polygonAggregateRequest = new PolygonAggregateRequest
        {
            Ticker = ticker,
            Multiplier = timeframe.Multiplier,
            Timespan = timeframe.Timespan.ToString(),
            From = start.ToString("yyyy-MM-dd"),
            To = DateTime.Now.ToString("yyyy-MM-dd"),
            Limit = 50000
        };

        var polygonAggregateResponse = await polygonClient.GetAggregates(polygonAggregateRequest);
        var stocksResponse = mapper.Map<StocksResponse>(polygonAggregateResponse);

        marketCache.SetStocksResponse(stocksResponse, timeframe, date);
    }

    private static TimeSpan GetStartOffset(Timespan timespan)
    {
        return timespan switch
        {
            Timespan.minute => TimeSpan.FromDays(-5),
            Timespan.hour => TimeSpan.FromDays(-30),
            Timespan.day => TimeSpan.FromDays(-365),
            Timespan.week => throw new NotImplementedException(),
            Timespan.month => throw new NotImplementedException(),
            Timespan.quarter => throw new NotImplementedException(),
            Timespan.year => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }

    private void SetSnapshot()
    {
        var now = DateTimeOffset.Now;
        var snapshotResponse = new SnapshotResponse
        {
            Entries = []
        };

        var minuteTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0, 0, now.Offset).AddMinutes(-1);
        var hourTime = new DateTimeOffset(minuteTime.Year, minuteTime.Month, minuteTime.Day, minuteTime.Hour, 0, 0, minuteTime.Offset);

        var tickers = marketCache.GetTickers();
        foreach (var ticker in tickers)
        {
            var minute = marketCache.GetStocksResponse(ticker, new Timeframe(1, Timespan.minute), DateTimeOffset.Now)?.Clone();
            var hour = marketCache.GetStocksResponse(ticker, new Timeframe(1, Timespan.hour), DateTimeOffset.Now)?.Clone();

            if (minute is null || hour is null)
            {
                continue;
            }

            var entry = snapshotResponse.Entries.FirstOrDefault(q => q.Ticker == ticker);

            if (entry is null || entry.Results is null)
            {
                snapshotResponse.Entries.Add(new SnapshotEntry
                {
                    Ticker = ticker,
                    Results = new List<Snapshot>()
                });
                entry = snapshotResponse.Entries.FirstOrDefault(q => q.Ticker == ticker);
            }

            if (ticker is "AAPL")
            {

            }

            entry.Results.Add(new Snapshot
            {
                Timestamp = minuteTime.ToUnixTimeMilliseconds(),
                DateTime = minuteTime,
                Minute = minute.Results?.FirstOrDefault(q => q.Timestamp == minuteTime.ToUnixTimeMilliseconds())?.Clone(),
                Hour = hour.Results?.FirstOrDefault(q => q.Timestamp == hourTime.ToUnixTimeMilliseconds())?.Clone()
            });
        }

        memoryCache.Set("snapshot", snapshotResponse);
    }
}
