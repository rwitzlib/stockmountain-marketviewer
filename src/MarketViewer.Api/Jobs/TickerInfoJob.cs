using Amazon.S3;
using Amazon.S3.Model;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
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

            var now = DateTimeOffset.Now;
            var startTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 1, 0, now.Offset);

            var initJob = JobBuilder.Create<InitialAggregateJob>()
                .StoreDurably(true)
                .Build();

            var snapshotTrigger = TriggerBuilder.Create()
                .ForJob(initJob)
                .StartAt(startTime)
                .Build();

            await context.Scheduler.ScheduleJob(initJob, snapshotTrigger);
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
        marketCache.SetTickersByTimeframe(date, new Timeframe(1, Timespan.minute), tickers);
        marketCache.SetTickersByTimeframe(date, new Timeframe(1, Timespan.hour), tickers);

        return tickers;
    }
}
