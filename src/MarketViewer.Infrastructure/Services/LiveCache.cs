using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using MarketViewer.Contracts.Responses;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Models;
using Timespan = MarketViewer.Contracts.Enums.Timespan;
using MarketViewer.Contracts.Models.ScanV2;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MarketViewer.Infrastructure.Services
{
    public class LiveCache(
        MarketCache _marketCache,
        ILogger<LiveCache> logger)
    {
        private const int MINIMUM_REQUIRED_CANDLES = 60;
        private const int CANDLES_TO_TAKE = 120;

        public IEnumerable<StocksResponse> GetStocksResponses(DateTimeOffset date)
        {
            if (date.DayOfWeek is DayOfWeek.Saturday || date.DayOfWeek is DayOfWeek.Sunday)
            {
                return [];
            }

            var stocksResponses = new List<StocksResponse>();

            var tickers = _marketCache.GetTickers(Timespan.minute, date);

            logger.LogInformation("Removing candles outside of {timestamp}.", date);
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

            logger.LogInformation("Returning {count} total aggregates.", stocksResponses.Count);
            return stocksResponses;
        }

        public StocksResponseCollection GetStocksResponses(DateTimeOffset date, IEnumerable<Timespan> timespans)
        {
            if (date.DayOfWeek is DayOfWeek.Saturday || date.DayOfWeek is DayOfWeek.Sunday)
            {
                return new StocksResponseCollection
                {
                    Responses = []
                };
            }

            var stocksResponseCollection = new StocksResponseCollection();

            foreach (var timespan in timespans)
            {
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
}
