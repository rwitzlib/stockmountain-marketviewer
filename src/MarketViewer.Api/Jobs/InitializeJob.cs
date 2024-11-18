using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using Polygon.Client.Requests;
using Quartz;
using System.Diagnostics;
using System.Text.Json;
using Polygon.Client.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace MarketViewer.Api.Jobs;

public class InitializeJob(
    IMemoryCache _memoryCache,
    MarketCache _marketCache,
    IAmazonS3 _amazonS3Client,
    IPolygonClient _polygonClient,
    IMapper _mapper,
    ILogger<InitializeJob> _logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        //_memoryCache.Dispose();
        //_memoryCache = new MemoryCache(new MemoryCacheOptions());
        var jobDataMap = context.JobDetail.JobDataMap;
        if (!jobDataMap.TryGetString("date", out var dateString))
        {
            _logger.LogInformation("Missing required data parameter.");
            return;
        }
        var date = DateTimeOffset.Parse(dateString);

        try
        {
            _logger.LogInformation("Started populating data at: {time}.", date);
            
            var sp = new Stopwatch();
            sp.Start();

            var tickers = await PopulateTickersAndTickerDetails(date);

            var batchSize = int.TryParse(Environment.GetEnvironmentVariable("BATCH_SIZE"), out int size) ? size : 500;

            await PopulateStocksResponses(tickers, batchSize, Timespan.minute, date);
            await PopulateStocksResponses(tickers, batchSize, Timespan.hour, date);

            sp.Stop();

            _logger.LogInformation("Finished populating at: {time}.", date);
            _logger.LogInformation("Time elapsed: {elapsed}ms.", sp.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error populating data: {message}", ex.Message);
        }
    }

    private async Task<IEnumerable<string>> PopulateTickersAndTickerDetails(DateTimeOffset date)
    {
        var request = new GetObjectRequest
        {
            BucketName = "lad-dev-marketviewer",
            Key = "tickerdetails/stocks.json"
        };
        var s3Response = await _amazonS3Client.GetObjectAsync(request);

        using var streamReader = new StreamReader(s3Response.ResponseStream);
        var json = await streamReader.ReadToEndAsync();

        var tickerDetailsList = JsonSerializer.Deserialize<IEnumerable<Polygon.Client.Models.TickerDetails>>(json);

        foreach (var tickerDetails in tickerDetailsList)
        {
            _marketCache.SetTickerDetails(tickerDetails);
        }

        var tickers = tickerDetailsList.Select(tickerDetails => tickerDetails.Ticker);

        _marketCache.SetTickers(tickers);
        _marketCache.SetTickersByTimespan(date, Timespan.minute, tickers);
        _marketCache.SetTickersByTimespan(date, Timespan.hour, tickers);

        _logger.LogInformation("Finished populating Tickers and TickerDetails.");

        return tickers;
    }

    private async Task PopulateStocksResponses(IEnumerable<string> tickers, int batchSize, Timespan timespan, DateTimeOffset date)
    {
        for (int i = 0; i < tickers.Count(); i += batchSize)
        {
            var tasks = new List<Task>();

            var batch = tickers.Take(new Range(i, i + batchSize));

            foreach (var ticker in batch)
            {
                tasks.Add(Task.Run(() => PopulateStocksResponse(ticker, 1, timespan, date)));
            }
            await Task.WhenAll(tasks);
        }
        _logger.LogInformation("Finished populating {timespan} data.", timespan);
    }

    private async Task PopulateStocksResponse(string ticker, int multiplier, Timespan timespan, DateTimeOffset date)
    {
        var start = date.Add(GetStartOffset(timespan));

        var polygonAggregateRequest = new PolygonAggregateRequest
        {
            Ticker = ticker,
            Multiplier = multiplier,
            Timespan = timespan.ToString(),
            From = start.ToUnixTimeMilliseconds().ToString(),
            To = date.ToUnixTimeMilliseconds().ToString(),
            Limit = 50000
        };
        var polygonAggregateResponse = await _polygonClient.GetAggregates(polygonAggregateRequest);

        var stocksResponse = _mapper.Map<StocksResponse>(polygonAggregateResponse);

        _marketCache.SetStocksResponse(stocksResponse, timespan, date);
    }

    private static TimeSpan GetStartOffset(Timespan timespan)
    {
        return timespan switch
        {
            Timespan.minute => TimeSpan.FromDays(-5),
            Timespan.hour => TimeSpan.FromDays(-60),
            Timespan.day => throw new NotImplementedException(),
            Timespan.week => throw new NotImplementedException(),
            Timespan.month => throw new NotImplementedException(),
            Timespan.quarter => throw new NotImplementedException(),
            Timespan.year => throw new NotImplementedException(),
            _ => throw new NotImplementedException()
        };
    }
}
