using Amazon.S3;
using Amazon.S3.Model;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Polygon.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MarketViewer.Infrastructure.Services;

public class HistoryCache(
    MarketCache _marketCache,
    ILogger<HistoryCache> _logger)
{
    private const int MINIMUM_REQUIRED_CANDLES = 30;
    private const int CANDLES_TO_TAKE = 120;

    public async Task<IEnumerable<StocksResponse>> GetStocksResponses(DateTimeOffset date)
    {
        var stocksResponses = new List<StocksResponse>();

        if (_marketCache.GetTickers(Timespan.minute, date) is null)
        {
            await _marketCache.Initialize(date.Date, 1, Timespan.minute);
        }

        var tickers = _marketCache.GetTickers(Timespan.minute, date);

        var time = date.ToUnixTimeMilliseconds();

        foreach (var ticker in tickers)
        {
            var stocksResponse = _marketCache.GetStocksResponse(ticker, Timespan.minute, date);

            if (stocksResponse.Results is null || stocksResponse.Results.Count < MINIMUM_REQUIRED_CANDLES)
            {
                continue;
            }

            var candles = stocksResponse.Results.Where(candle => candle.Timestamp <= time).TakeLast(CANDLES_TO_TAKE);

            var tickerDetails = _marketCache.GetTickerDetails(ticker);
            stocksResponses.Add(new StocksResponse
            {
                Ticker = ticker,
                Results = candles.ToList(),
                TickerDetails = tickerDetails
            });
        }

        return stocksResponses;
    }

    public async Task<StocksResponseCollection> GetStocksResponsesV2(DateTimeOffset date, IEnumerable<Timespan> timespans)
    {
        var stocksResponseCollection = new StocksResponseCollection();

        foreach (var timespan in timespans)
        {
            if (_marketCache.GetTickers(timespan, date) is null)
            {
                await _marketCache.Initialize(date, 1, timespan); //TODO add multiplier input eventually
            }

            var tickers = _marketCache.GetTickers(timespan, date);

            var time = date.ToUnixTimeMilliseconds();

            var stocksResponses = new List<StocksResponse>();

            foreach (var ticker in tickers)
            {
                var stocksResponse = _marketCache.GetStocksResponse(ticker, timespan, date);

                if (stocksResponse.Results is null || stocksResponse.Results.Count < MINIMUM_REQUIRED_CANDLES)
                {
                    continue;
                }

                var candles = stocksResponse.Results.Where(candle => candle.Timestamp <= time).TakeLast(CANDLES_TO_TAKE);

                var tickerDetails = _marketCache.GetTickerDetails(ticker);
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
}
