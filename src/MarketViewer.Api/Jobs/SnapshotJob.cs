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
using Snapshot = MarketViewer.Contracts.Models.Snapshot.Snapshot;
using MarketViewer.Contracts.Responses.Tools;

namespace MarketViewer.Api.Jobs;

public class SnapshotJob(
    IMemoryCache memoryCache,
    IMarketCache marketCache,
    IPolygonClient polygonClient,
    ILogger<SnapshotJob> logger) : IJob
{
    private readonly Stopwatch _sp = new();

    private readonly TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    public async Task Execute(IJobExecutionContext context)
    {
        _sp.Start();

        try
        {
            logger.LogInformation("Started snapshot job at: {time}.", DateTimeOffset.Now);

            var polygonSnapshotResponse = await polygonClient.GetAllTickersSnapshot(null);

            var snapshotResponse = memoryCache.Get<SnapshotResponse>("snapshot");

            foreach (var snapshot in polygonSnapshotResponse.Tickers)
            {
                var minuteCandle = AddBarToCache(snapshot.Ticker, new Timeframe(1, Timespan.minute), snapshot.Minute);
                var hourCandle = AddBarToCache(snapshot.Ticker, new Timeframe(1, Timespan.hour), snapshot.Minute);

                var snapshotEntry = snapshotResponse.Entries.FirstOrDefault(q => q.Ticker == snapshot.Ticker);

                if (snapshotEntry is null || snapshotEntry.Results is null)
                {
                    continue;
                }

                var offset = _timeZone.GetUtcOffset(DateTimeOffset.FromUnixTimeMilliseconds(snapshot.Minute.Timestamp));

                snapshotEntry.Results.Add(new Snapshot
                {
                    Timestamp = snapshot.Minute.Timestamp,
                    DateTime = DateTimeOffset.FromUnixTimeMilliseconds(snapshot.Minute.Timestamp).ToOffset(offset),
                    Minute = minuteCandle?.Clone(),
                    Hour = hourCandle?.Clone()
                });
            }

            memoryCache.Set("snapshot", snapshotResponse);

            _sp.Stop();
            logger.LogInformation("Finished snapshot job at: {time}. Time elapsed: {elapsed}ms.", DateTimeOffset.Now, _sp.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError("Error during snapshot job: {message}", ex.Message);
        }
    }

    private Bar AddBarToCache(string ticker, Timeframe timeframe, Bar newCandle)
    {
        var stocksResponse = marketCache.GetStocksResponse(ticker, timeframe, DateTimeOffset.Now);

        if (stocksResponse is null || !stocksResponse.Results.Any())
        {
            return null;
        }

        var lastCandle = stocksResponse.Results.Last();

        if (lastCandle.Timestamp >= newCandle.Timestamp)
        {
            return null;
        }

        switch (timeframe.Timespan)
        {
            case Timespan.minute:
                stocksResponse.Results.Add(newCandle.Clone());
                return newCandle.Clone();

            case Timespan.hour:
                var lastDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(lastCandle.Timestamp);
                var currentDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(newCandle.Timestamp);

                if (lastDateTimeOffset.Hour < currentDateTimeOffset.Hour)
                {
                    stocksResponse.Results.Add(newCandle.Clone());
                }
                else
                {
                    if (newCandle.Volume == lastCandle.Volume && newCandle.TransactionCount == lastCandle.TransactionCount)
                    {
                        // There hasnt been a new candle yet so dont update
                        return null;
                    }

                    if (newCandle.High > lastCandle.High)
                    {
                        lastCandle.High = newCandle.High;
                    }

                    if (newCandle.Low < lastCandle.Low)
                    {
                        lastCandle.Low = newCandle.Low;
                    }


                    lastCandle.Close = newCandle.Close;

                    // TODO: How to do a more precise VWAP?
                    lastCandle.Vwap = (newCandle.Close + newCandle.High + newCandle.Low) / 3;

                    lastCandle.Volume += newCandle.Volume;
                    lastCandle.TransactionCount += newCandle.TransactionCount;
                }
                return lastCandle;

            default:
                return null;
        }
    }
}
