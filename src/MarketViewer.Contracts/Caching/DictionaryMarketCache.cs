using Amazon.S3;
using Amazon.S3.Model;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using Polygon.Client.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MarketViewer.Contracts.Caching;

public class DictionaryMarketCache(IAmazonS3 _amazonS3) : IMarketCache
{
    private readonly ConcurrentDictionary<string, IEnumerable<string>> _tickers = new();
    private readonly ConcurrentDictionary<string, TickerDetails> _tickerDetails = new();
    private readonly ConcurrentDictionary<string, StocksResponse> _stocksResponses = new();

    private readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<StocksResponse>> Initialize(DateTimeOffset date, int multiplier, Timespan timespan)
    {
        var s3Request = new GetObjectRequest
        {
            BucketName = "lad-dev-marketviewer",
            Key = BuildS3Key(date, multiplier, timespan)
        };

        var s3Response = await _amazonS3.GetObjectAsync(s3Request);

        using var streamReader = new StreamReader(s3Response.ResponseStream);
        var json = await streamReader.ReadToEndAsync();

        var stocksResponses = JsonSerializer.Deserialize<IEnumerable<StocksResponse>>(json, Options);

        var tickers = stocksResponses.Select(stocksResponse => stocksResponse.Ticker);

        SetTickersByTimespan(date, timespan, tickers); //TODO use multiplier in cache key eventually?

        foreach (var stocksResponse in stocksResponses)
        {
            SetStocksResponse(stocksResponse, timespan, date); //TODO use multiplier in cache key eventually?
        }

        return stocksResponses;
    }

    public IEnumerable<string> GetTickers()
    {
        if (_tickers.TryGetValue("Tickers", out IEnumerable<string> tickers) && tickers is not null)
        {
            return tickers;
        }

        return [];
    }

    public void SetTickers(IEnumerable<string> tickers)
    {
        _tickers.AddOrUpdate("Tickers", tickers, (s, t) => t);
    }

    public IEnumerable<string> GetTickersByTimespan(Timespan timespan, DateTimeOffset timestamp)
    {
        if (_tickers.TryGetValue($"Tickers/{timespan}/{timestamp.Date:yyyyMMdd}", out var tickers) && tickers is not null)
        {
            return tickers;
        }

        return [];
    }

    public void SetTickersByTimespan(DateTimeOffset date, Timespan timespan, IEnumerable<string> tickers)
    {
        _tickers.AddOrUpdate($"Tickers/{timespan}/{date.Date:yyyyMMdd}", tickers, (s, t) => t);
    }

    public StocksResponse GetStocksResponse(string ticker, Timespan timespan, DateTimeOffset timestamp)
    {
        if (_stocksResponses.TryGetValue($"Stocks/{ticker}/{timespan}/{timestamp.Date:yyyyMMdd}", out var stocksResponse))
        {
            return stocksResponse;
        }

        return null;
    }

    public void SetStocksResponse(StocksResponse stocksResponse, Timespan timespan, DateTimeOffset date)
    {
        _stocksResponses.AddOrUpdate($"Stocks/{stocksResponse.Ticker}/{timespan}/{date.Date:yyyyMMdd}", stocksResponse, (s, t) => t);
    }

    public TickerDetails GetTickerDetails(string ticker)
    {
        if (_tickerDetails.TryGetValue($"TickerDetails/{ticker}", out var tickerDetails))
        {
            return tickerDetails;
        }

        return null;
    }

    public void SetTickerDetails(TickerDetails tickerDetails)
    {
        _tickerDetails.AddOrUpdate($"TickerDetails/{tickerDetails.Ticker}", tickerDetails, (s, t) => t);
    }

    private static string BuildS3Key(DateTimeOffset timestamp, int multiplier, Timespan timespan)
    {
        var month = timestamp.Date.Month < 10 ? $"0{timestamp.Date.Month}" : $"{timestamp.Date.Month}";
        var day = timestamp.Date.Day < 10 ? $"0{timestamp.Date.Day}" : $"{timestamp.Date.Day}";

        return timespan switch
        {
            Timespan.minute => $"backtest/{timestamp.Date.Year}/{month}/{day}/aggregate_{multiplier}_{timespan}",
            Timespan.hour => $"backtest/{timestamp.Date.Year}/{month}/aggregate_{multiplier}_{timespan}",
            _ => throw new NotImplementedException()
        };
    }
}
