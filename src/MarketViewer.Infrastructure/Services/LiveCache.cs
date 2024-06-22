using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using MarketViewer.Contracts.Responses;
using Microsoft.Extensions.Caching.Memory;
using Polygon.Client.Models;
using Timespan = MarketViewer.Contracts.Enums.Timespan;
using MarketViewer.Contracts.Models.ScanV2;

namespace MarketViewer.Infrastructure.Services
{
    public class LiveCache(
        IMemoryCache memoryCache,
        ILogger<LiveCache> logger)
    {
        private const int MINIMUM_REQUIRED_CANDLES = 60;
        private const int CANDLES_TO_TAKE = 120;

        public IEnumerable<StocksResponse> GetStocksResponses(DateTimeOffset timestamp)
        {
            if (timestamp.DayOfWeek is DayOfWeek.Saturday || timestamp.DayOfWeek is DayOfWeek.Sunday)
            {
                return [];
            }

            var stocksResponses = new List<StocksResponse>();

            var tickers = memoryCache.Get<IEnumerable<string>>("Tickers");

            logger.LogInformation("Removing candles outside of {timestamp}.", timestamp);
            var time = timestamp.ToUnixTimeMilliseconds();

            foreach (var ticker in tickers)
            {
                var stocksResponse = memoryCache.Get<StocksResponse>($"Stock_{ticker}_{DateTime.Now:yyyyMMdd}");

                if (stocksResponse.Results is null || stocksResponse.Results.Count < MINIMUM_REQUIRED_CANDLES)
                {
                    continue;
                }

                var candles = stocksResponse.Results.Where(candle => candle.Timestamp <= time).TakeLast(CANDLES_TO_TAKE);

                var tickerDetails = memoryCache.Get<TickerDetails>($"TickerDetails_{ticker}");

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

        public StocksResponseCollection GetStocksResponses(DateTimeOffset timestamp, IEnumerable<Timespan> timespans)
        {
            return null;
        }
    }
}
