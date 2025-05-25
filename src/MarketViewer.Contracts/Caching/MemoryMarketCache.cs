using Amazon.S3;
using Amazon.S3.Model;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Responses.Market;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Models;
using System.Text.Json;

namespace MarketViewer.Contracts.Caching;

public class MemoryMarketCache(IMemoryCache _memoryCache, IAmazonS3 _amazonS3) : IMarketCache
{
    private readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static TimeSpan ExpireIn => TimeSpan.FromHours(16);

    public async Task<IEnumerable<StocksResponse>> Initialize(DateTimeOffset date, Timeframe timeframe)
    {
        var s3Request = new GetObjectRequest
        {
            BucketName = "lad-dev-marketviewer",
            Key = BuildS3Key(date, timeframe.Multiplier, timeframe.Timespan)
        };

        using var s3Response = await _amazonS3.GetObjectAsync(s3Request);
        using var streamReader = new StreamReader(s3Response.ResponseStream);

        var json = await streamReader.ReadToEndAsync();

        var stocksResponses = JsonSerializer.Deserialize<IEnumerable<StocksResponse>>(json, Options);

        var tickers = stocksResponses.Select(stocksResponse => stocksResponse.Ticker);

        SetTickersByTimeframe(date, timeframe, tickers); //TODO use multiplier in cache key eventually?

        foreach (var stocksResponse in stocksResponses)
        {
            SetStocksResponse(stocksResponse, timeframe, date); //TODO use multiplier in cache key eventually?
        }

        return stocksResponses;
    }

    public IEnumerable<string> GetTickers()
    {
        return _memoryCache.Get<IEnumerable<string>>("Tickers");
    }

    public void SetTickers(IEnumerable<string> tickers)
    {
        _memoryCache.Set("Tickers", tickers);
    }

    public IEnumerable<string> GetTickersByTimeframe(Timeframe timeframe, DateTimeOffset timestamp)
    {
        return _memoryCache.Get<IEnumerable<string>>($"Tickers/{timeframe.Multiplier}/{timeframe.Timespan}/{timestamp.Date:yyyyMMdd}");
    }

    public void SetTickersByTimeframe(DateTimeOffset date, Timeframe timeframe, IEnumerable<string> tickers)
    {
        _memoryCache.GetOrCreate($"Tickers/{timeframe.Multiplier}/{timeframe.Timespan}/{date.Date:yyyyMMdd}", entry =>
        {
            entry.SetSlidingExpiration(ExpireIn);
            return tickers;
        });
    }

    public StocksResponse GetStocksResponse(string ticker, Timeframe timeframe, DateTimeOffset timestamp)
    {
        return _memoryCache.Get<StocksResponse>($"Stocks/{ticker}/{timeframe.Multiplier}/{timeframe.Timespan}/{timestamp.Date:yyyyMMdd}");
    }

    public void SetStocksResponse(StocksResponse stocksResponse, Timeframe timeframe, DateTimeOffset date)
    {
        if (stocksResponse is null)
        {
            return;
        }

        _memoryCache.GetOrCreate($"Stocks/{stocksResponse.Ticker}/{timeframe.Multiplier}/{timeframe.Timespan}/{date.Date:yyyyMMdd}", entry =>
        {
            entry.SetSlidingExpiration(ExpireIn);
            return stocksResponse;
        });
    }

    public TickerDetails GetTickerDetails(string ticker)
    {
        return _memoryCache.Get<TickerDetails>($"TickerDetails/{ticker}");
    }

    public void SetTickerDetails(TickerDetails tickerDetails)
    {
        _memoryCache.Set($"TickerDetails/{tickerDetails.Ticker}", tickerDetails);
    }

    private static string BuildS3Key(DateTimeOffset timestamp, int multiplier, Timespan timespan)
    {
        var month = timestamp.Date.Month < 10 ? $"0{timestamp.Date.Month}" : $"{timestamp.Date.Month}";
        var day = timestamp.Date.Day < 10 ? $"0{timestamp.Date.Day}" : $"{timestamp.Date.Day}";

        return timespan switch
        {
            Timespan.minute => $"backtest/{timestamp.Date.Year}/{month}/{day}/aggregate_{multiplier}_{timespan}",
            Timespan.hour => $"backtest/{timestamp.Date.Year}/{month}/aggregate_{multiplier}_{timespan}",
            Timespan.day => $"backtest/{timestamp.Date.Year}/aggregate_{multiplier}_{timespan}",
            _ => throw new NotImplementedException()
        };
    }
}
