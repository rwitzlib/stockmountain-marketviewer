using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketViewer.Contracts.Responses;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polygon.Client.Models;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.ScanV2;

namespace MarketViewer.Infrastructure.Services;

public class HistoryCache(
    IMemoryCache memoryCache,
    IAmazonS3 amazonS3Client,
    ILogger<HistoryCache> logger)
{
    private const int MINIMUM_REQUIRED_CANDLES = 30;
    private const int CANDLES_TO_TAKE = 120;

    private readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task InitializeCache(DateTime date, int multiplier, Timespan timespan)
    {
        var s3Request = new GetObjectRequest
        {
            BucketName = "lad-dev-marketviewer",
            Key = BuildS3Key(date, multiplier, timespan)
        };

        var s3Response = await amazonS3Client.GetObjectAsync(s3Request);

        using var streamReader = new StreamReader(s3Response.ResponseStream);
        var json = await streamReader.ReadToEndAsync();

        var stocksResponses = JsonSerializer.Deserialize<IEnumerable<StocksResponse>>(json, Options);
        
        var tickers = stocksResponses.Select(stocksResponse => stocksResponse.Ticker);
        memoryCache.GetOrCreate($"Tickers/{timespan}/{date:yyyyMMdd}", entry =>
        {
            entry.SetSlidingExpiration(TimeSpan.FromMinutes(5));

            return tickers;
        });

        foreach (var stocksResponse in stocksResponses)
        {
            memoryCache.GetOrCreate($"Stocks/{stocksResponse.Ticker}/{timespan}/{date:yyyyMMdd}", entry => //TODO add handling for multipliers
            {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(5));

                return stocksResponse;
            });
        }
    }

    public async Task<IEnumerable<StocksResponse>> GetStocksResponses(DateTimeOffset timestamp)
    {
        var stocksResponses = new List<StocksResponse>();

        if (memoryCache.Get<IEnumerable<string>>($"Tickers/minute/{timestamp.Date:yyyyMMdd}") is null)
        {
            await InitializeCache(timestamp.Date, 1, Timespan.minute);
        }

        var tickers = memoryCache.Get<IEnumerable<string>>($"Tickers/minute/{timestamp.Date:yyyyMMdd}");

        //logger.LogInformation("Removing candles outside of {timestamp}.", timestamp);
        var time = timestamp.ToUnixTimeMilliseconds();

        foreach (var ticker in tickers)
        {
            var stocksResponse = memoryCache.Get<StocksResponse>($"Stocks/{ticker}/minute/{timestamp.Date:yyyyMMdd}");

            if (stocksResponse.Results is null || stocksResponse.Results.Count < MINIMUM_REQUIRED_CANDLES)
            {
                continue;
            }

            var candles = stocksResponse.Results.Where(candle => candle.Timestamp <= time).TakeLast(CANDLES_TO_TAKE);

            var tickerDetails = memoryCache.Get<TickerDetails>($"TickerDetails/{ticker}");
            stocksResponses.Add(new StocksResponse
            {
                Ticker = ticker,
                Results = candles.ToList(),
                TickerDetails = tickerDetails
            });
        }

        //logger.LogInformation("Returning {count} total aggregates.", stocksResponses.Count);

        return stocksResponses;
    }

    public async Task<StocksResponseCollection> GetStocksResponsesV2(DateTimeOffset timestamp, IEnumerable<Timespan> timespans)
    {
        var stocksResponseCollection = new StocksResponseCollection();

        foreach (var timespan in timespans)
        {
            if (memoryCache.Get<IEnumerable<string>>($"Tickers/{timespan}/{timestamp.Date:yyyyMMdd}") is null)
            {
                await InitializeCache(timestamp.Date, 1, timespan); //TODO add multiplier input eventually
            }

            var tickers = memoryCache.Get<IEnumerable<string>>($"Tickers/{timespan}/{timestamp.Date:yyyyMMdd}");

            var time = timestamp.ToUnixTimeMilliseconds();

            var stocksResponses = new List<StocksResponse>();

            foreach (var ticker in tickers)
            {
                var stocksResponse = memoryCache.Get<StocksResponse>($"Stocks/{ticker}/{timespan}/{timestamp.Date:yyyyMMdd}");

                if (stocksResponse.Results is null || stocksResponse.Results.Count < MINIMUM_REQUIRED_CANDLES)
                {
                    continue;
                }

                var candles = stocksResponse.Results.Where(candle => candle.Timestamp <= time).TakeLast(CANDLES_TO_TAKE);

                var tickerDetails = memoryCache.Get<TickerDetails>($"TickerDetails/{ticker}");
                stocksResponses.Add(new StocksResponse
                {
                    Ticker = ticker,
                    Results = candles.ToList(),
                    TickerDetails = tickerDetails
                });
            }

            if (stocksResponses.Count != 0)
            {
                stocksResponseCollection.Responses.Add(timespan, stocksResponses);
            }
        }

        return stocksResponseCollection;
    }

    private static string BuildS3Key(DateTimeOffset timestamp, int multiplier, Timespan timespan)
    {
        var month = timestamp.Date.Month < 10 ? $"0{timestamp.Date.Month}" : $"{timestamp.Date.Month}";
        var day = timestamp.Date.Month < 10 ? $"0{timestamp.Date.Day}" : $"{timestamp.Date.Day}";

        return timespan switch
        {
            Timespan.minute => $"backtest/{timestamp.Date.Year}/{month}/{day}/aggregate_{multiplier}_{timespan}",
            Timespan.hour => $"backtest/{timestamp.Date.Year}/{month}/aggregate_{multiplier}_{timespan}",
            _ => throw new NotImplementedException()
        };
    }
}
