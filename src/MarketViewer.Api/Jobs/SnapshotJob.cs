using AutoMapper;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using Quartz;
using System.Diagnostics;
using Polygon.Client.Interfaces;
using Polygon.Client.Models;
using MarketViewer.Contracts.Models.Scan;

namespace MarketViewer.Api.Jobs;

public class SnapshotJob(
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

            foreach (var snapshot in snapshotResponse.Tickers)
            {
                AddBarToCache(snapshot.Ticker, new Timeframe(1, Timespan.minute), snapshot.Minute);
                AddBarToCache(snapshot.Ticker, new Timeframe(1, Timespan.hour), snapshot.Minute);
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
                    stocksResponse.Results.Add(currentCandle);
                    return;
                case Timespan.hour:
                    var lastDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(lastCandle.Timestamp);
                    var currentDateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(currentCandle.Timestamp);

                    if ((lastDateTimeOffset.Hour + 1) == currentDateTimeOffset.Hour)
                    {
                        if (ticker == "SPY")
                        {

                        }
                        stocksResponse.Results.Add(currentCandle);
                    }
                    else
                    {
                        if (currentCandle.Volume == lastCandle.Volume && currentCandle.TransactionCount == lastCandle.TransactionCount)
                        {
                            // There hasnt been a new candle yet so dont update
                            break;
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
}
