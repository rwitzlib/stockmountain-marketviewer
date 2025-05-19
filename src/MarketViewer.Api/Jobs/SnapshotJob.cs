using AutoMapper;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using Quartz;
using System.Diagnostics;
using Polygon.Client.Interfaces;
using Polygon.Client.Models;
using Polygon.Client.Requests;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Responses;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Responses;
using NRedisStack.Search.Aggregation;
using Snapshot = MarketViewer.Contracts.Models.Snapshot.Snapshot;

namespace MarketViewer.Api.Jobs;

public class SnapshotJob(
    IMemoryCache memoryCache,
    IMarketCache marketCache,
    IPolygonClient polygonClient,
    ILogger<SnapshotJob> logger) : IJob
{
    private readonly Stopwatch _sp = new();

    public async Task Execute(IJobExecutionContext context)
    {
        _sp.Start();

        try
        {
            logger.LogInformation("Started snapshot job at: {time}.", DateTimeOffset.Now);

            var polygonSnapshotResponse = await polygonClient.GetAllTickersSnapshot(null);

            foreach (var snapshot in polygonSnapshotResponse.Tickers)
            {
                AddBarToCache(snapshot.Ticker, new Timeframe(1, Timespan.minute), snapshot.Minute);
                AddBarToCache(snapshot.Ticker, new Timeframe(1, Timespan.hour), snapshot.Minute);
            }

            if (DateTimeOffset.Now.Hour < 15)
            {
                SetSnapshot();
            }

            _sp.Stop();
            logger.LogInformation("Finished snapshot job at: {time}. Time elapsed: {elapsed}ms.", DateTimeOffset.Now, _sp.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError("Error during snapshot job: {message}", ex.Message);
        }
    }

    private void AddBarToCache(string ticker, Timeframe timeframe, Bar currentCandle)
    {
        if (currentCandle.Timestamp == 0)
        {
            return;
        }

        var stocksResponse = marketCache.GetStocksResponse(ticker, timeframe, DateTimeOffset.Now);

        if (stocksResponse is null || !stocksResponse.Results.Any())
        {
            return;
        }

        var lastCandle = stocksResponse.Results.Last();
        if (lastCandle.Timestamp < currentCandle.Timestamp)
        {
            switch (timeframe.Timespan)
            {
                case Timespan.minute:
                    stocksResponse.Results.Add(currentCandle.Clone());
                    return;

                case Timespan.hour:
                    var lastDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(lastCandle.Timestamp);
                    var currentDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(currentCandle.Timestamp);

                    if (lastDateTimeOffset.Hour < currentDateTimeOffset.Hour)
                    {
                        stocksResponse.Results.Add(currentCandle.Clone());
                    }
                    else
                    {
                        if (currentCandle.Volume == lastCandle.Volume && currentCandle.TransactionCount == lastCandle.TransactionCount)
                        {
                            // There hasnt been a new candle yet so dont update
                            return;
                        }

                        if (currentCandle.High > lastCandle.High)
                        {
                            lastCandle.High = currentCandle.High;
                        }

                        if (currentCandle.Low < lastCandle.Low)
                        {
                            lastCandle.Low = currentCandle.Low;
                        }


                        lastCandle.Close = currentCandle.Close;

                        // TODO: How to do a more precise VWAP?
                        lastCandle.Vwap = (currentCandle.Close + currentCandle.High + currentCandle.Low) / 3;

                        lastCandle.Volume += currentCandle.Volume;
                        lastCandle.TransactionCount += currentCandle.TransactionCount;
                    }                    
                    return;
            }
        }
    }

    private void SetSnapshot()
    {
        var snapshotResponse = memoryCache.Get<SnapshotResponse>("snapshot");

        var now = DateTimeOffset.Now;
        var minuteTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.Minute - 1, 0, 0, now.Offset);
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
                return;
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
