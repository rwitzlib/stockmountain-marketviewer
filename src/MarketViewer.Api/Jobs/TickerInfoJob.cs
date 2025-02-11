using Amazon.S3;
using Amazon.S3.Model;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Models;
using Quartz;
using System.Diagnostics;
using System.Text.Json;

namespace MarketViewer.Api.Jobs;

public class TickerInfoJob(
    IMemoryCache memoryCache, 
    IMarketCache marketCache,
    IAmazonS3 s3Client,
    ILogger<TickerInfoJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var sp = new Stopwatch();
        sp.Start();

        try
        {
            if (memoryCache is MemoryCache cache)
            {
                cache.Clear();
            }

            var date = DateTimeOffset.Now;

            logger.LogInformation("Started populating ticker data at: {time}.", date);

            await PopulateTickersAndTickerDetails(date);

            sp.Stop();

            logger.LogInformation("Finished populating ticker data at: {time}. Time elapsed: {elapsed}ms.", date, sp.ElapsedMilliseconds);

            List<Timespan> timespans = [
                Timespan.minute,
                Timespan.hour
            ];

            foreach (var timespan in timespans)
            {
                var now = DateTimeOffset.Now;
                var startTime = timespan switch
                {
                    Timespan.minute => new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 1, 0, now.Offset),
                    Timespan.hour => new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(2).Minute, 1, 0, now.Offset),
                    _ => new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 1, 0, now.Offset)
                };

                if (startTime.Minute >= 59)
                {
                    startTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.AddHours(1).Hour, 1, 1, 0, now.Offset);
                }

                var initJob = JobBuilder.Create<InitialAggregateJob>()
                    .WithIdentity($"Aggregate-{timespan}-{Guid.NewGuid()}")
                    .UsingJobData("timespan", timespan.ToString())
                    .StoreDurably(true)
                    .Build();

                var snapshotTrigger = TriggerBuilder.Create()
                    .WithIdentity($"InitializeTrigger-{timespan}-{Guid.NewGuid()}")
                    .ForJob(initJob)
                    .StartAt(startTime)
                    .Build();

                await context.Scheduler.ScheduleJob(initJob, snapshotTrigger);
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error populating ticker data: {message}", ex.Message);
        }
    }

    private async Task<IEnumerable<string>> PopulateTickersAndTickerDetails(DateTimeOffset date)
    {
        var request = new GetObjectRequest
        {
            BucketName = "lad-dev-marketviewer",
            Key = "tickerdetails/stocks.json"
        };
        var s3Response = await s3Client.GetObjectAsync(request);

        using var streamReader = new StreamReader(s3Response.ResponseStream);
        var json = await streamReader.ReadToEndAsync();

        var tickerDetailsList = JsonSerializer.Deserialize<IEnumerable<TickerDetails>>(json);

        foreach (var tickerDetails in tickerDetailsList)
        {
            marketCache.SetTickerDetails(tickerDetails);
        }

        var tickers = tickerDetailsList.Select(tickerDetails => tickerDetails.Ticker);

        marketCache.SetTickers(tickers);
        marketCache.SetTickersByTimespan(date, Timespan.minute, tickers);
        marketCache.SetTickersByTimespan(date, Timespan.hour, tickers);

        return tickers;
    }
}
