﻿using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Study;
using MarketViewer.Contracts.Responses.Market;

namespace MarketViewer.Studies.Studies;

public class RVOL(IMarketCache marketCache) : IStudy
{
    private readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    public List<List<LineEntry>> Compute(string[] parameters, ref StocksResponse stocksResponse)
    {
        var results = new List<List<LineEntry>>
        {
            new()
        };

        if (parameters is not null)
        {
            if (parameters.Any())
            {
                return [];
            }
        }

        var dailyResponse = marketCache.GetStocksResponse(stocksResponse.Ticker, new Timeframe(1, Timespan.day), DateTimeOffset.Now);

        if (dailyResponse is null || stocksResponse is null)
        {
            return [];
        }

        if (stocksResponse.TickerInfo.AverageVolume == 0)
        {
            stocksResponse.TickerInfo.AverageVolume = dailyResponse.Results.Select(result => result.Volume).Average();
        }

        float currentVolume = 0;
        for (int i = 0; i < stocksResponse.Results.Count; i++)
        {
            var offset = TimeZone.IsDaylightSavingTime(DateTimeOffset.FromUnixTimeMilliseconds(stocksResponse.Results[i].Timestamp)) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);

            if (results[0].Count == 0 || DateTimeOffset.FromUnixTimeMilliseconds(stocksResponse.Results[i].Timestamp).ToOffset(offset).Day != DateTimeOffset.FromUnixTimeMilliseconds(stocksResponse.Results[i - 1].Timestamp).ToOffset(offset).Day)
            {
                currentVolume = 0;
            }
            currentVolume += stocksResponse.Results[i].Volume;
            results[0].Add(new LineEntry
            {
                Timestamp = stocksResponse.Results[i].Timestamp,
                Value = currentVolume / stocksResponse.TickerInfo.AverageVolume
            });
        }

        return results;
    }
}