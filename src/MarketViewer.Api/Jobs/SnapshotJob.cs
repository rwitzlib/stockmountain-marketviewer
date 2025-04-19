using AutoMapper;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using Quartz;
using System.Diagnostics;
using Polygon.Client.Interfaces;
using Polygon.Client.Models;
using MarketViewer.Contracts.Models.Scan;
using Polygon.Client.Requests;
using Microsoft.Extensions.Caching.Memory;
using MarketViewer.Contracts.Entities;
using Polygon.Client.Responses;

namespace MarketViewer.Api.Jobs;

public class SnapshotJob(
    IMemoryCache memoryCache,
    IMarketCache marketCache,
    IPolygonClient polygonClient,
    IMapper mapper,
    ILogger<SnapshotJob> logger) : IJob
{
    private readonly Stopwatch _sp = new();

    public async Task Execute(IJobExecutionContext context)
    {
        _sp.Start();

        try
        {
            logger.LogInformation("Started snapshot job at: {time}.", DateTimeOffset.Now);

            var snapshotResponse = await polygonClient.GetAllTickersSnapshot(null);
            var timestamp = snapshotResponse.Tickers.FirstOrDefault(q => q.Ticker == "SPY")?.Minute.Timestamp;
            var datetime = DateTimeOffset.FromUnixTimeMilliseconds(timestamp.Value).ToOffset(TimeSpan.FromHours(-5));

            var minute = memoryCache.Get<PolygonFidelity>("SPY_minute");
            var minuteResponse = await polygonClient.GetAggregates(new PolygonAggregateRequest
            {
                Ticker = "SPY",
                Multiplier = 1,
                Timespan = "minute",
                From = DateTimeOffset.Now.AddDays(-1).ToString("yyyy-MM-dd"),
                To = DateTimeOffset.Now.ToString("yyyy-MM-dd"),
            });

            minute.Snapshots.Add(datetime, new PolygonSnapshotResponse
            {
                RequestId = snapshotResponse.RequestId,
                Count = 1,
                Tickers = snapshotResponse.Tickers.Where(q => q.Ticker == "SPY"),
                Status = snapshotResponse.Status
            });
            minute.Aggregates.Add(datetime, new PolygonAggregateResponse
            {
                RequestId = minuteResponse.RequestId,
                Status = minuteResponse.Status,
                Ticker = minuteResponse.Ticker,
                Results = minuteResponse.Results.Where(q => q.Timestamp == timestamp)
            });
            memoryCache.Set("SPY_minute", minute);

            var hourResponse = await polygonClient.GetAggregates(new PolygonAggregateRequest
            {
                Ticker = "SPY",
                Multiplier = 1,
                Timespan = "hour",
                From = DateTimeOffset.Now.AddDays(-1).ToString("yyyy-MM-dd"),
                To = DateTimeOffset.Now.ToString("yyyy-MM-dd"),
            });

            var hour = memoryCache.Get<PolygonFidelity>("SPY_hour");
            hour.Snapshots.Add(datetime, new PolygonSnapshotResponse
            {
                RequestId = snapshotResponse.RequestId,
                Count = 1,
                Tickers = snapshotResponse.Tickers.Where(q => q.Ticker == "SPY"),
                Status = snapshotResponse.Status
            });
            hour.Aggregates.Add(datetime, new PolygonAggregateResponse
            {
                RequestId = hourResponse.RequestId,
                Status = hourResponse.Status,
                Ticker = hourResponse.Ticker,
                Results = hourResponse.Results.Where(q => q.Timestamp == timestamp)
            });
            memoryCache.Set("SPY_hour", hour);

            foreach (var snapshot in snapshotResponse.Tickers)
            {
                AddBarToCache(snapshot.Ticker, new Timeframe(1, Timespan.minute), snapshot.Minute);
                AddBarToCache(snapshot.Ticker, new Timeframe(1, Timespan.hour), snapshot.Minute);
            }

            var snapshotVolume = minute.Snapshots.Last().Value.Tickers.FirstOrDefault().Minute.Volume;
            var aggregateVolume = minute.Aggregates.Last().Value.Results.FirstOrDefault().Volume;

            if (snapshotVolume != aggregateVolume)
            {
                logger.LogInformation("Value mismatch between snapshot and aggregate at {time}.", datetime);
                logger.LogInformation("Snapshot: {snapshot}", minute.Snapshots.Last());
                logger.LogInformation("Aggregate: {aggregate}", minute.Aggregates.Last());
            }

            _sp.Stop();
            logger.LogInformation("Finished snapshot job at: {time}. Time elapsed: {elapsed}ms.", DateTimeOffset.Now, _sp.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError("Error during snapshot job: {message}", ex.Message);
        }
    }

    public void AddBarToCache(string ticker, Timeframe timeframe, Bar currentCandle)
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

                    if (ticker is "SPY")
                    {
                        var minuteFidelity = memoryCache.Get<PolygonFidelity>("SPY_minute");
                        minuteFidelity.Data = stocksResponse;
                        memoryCache.Set("SPY_minute", minuteFidelity);
                    }

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

                    if (ticker is "SPY")
                    {
                        var hourfidelity = memoryCache.Get<PolygonFidelity>("SPY_hour");
                        hourfidelity.Data = stocksResponse;
                        memoryCache.Set("SPY_hour", hourfidelity);
                    }
                    
                    return;
            }
        }
    }
}
