using Amazon.S3;
using Amazon.S3.Model;
using MarketViewer.Contracts.Caching;
using Polygon.Client.Models;
using System.Diagnostics;
using System.Text.Json;

namespace MarketViewer.Api.HostedServices;

public class PopulateMarketData(
    MarketCache _marketCache,
    IAmazonS3 amazonS3Client,
    ILogger<PopulateMarketData> logger) : IHostedLifecycleService
{
    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting");

        var sp = new Stopwatch();
        sp.Start();
        logger.LogInformation("TickerDetails - Started populating at: {time}.", DateTimeOffset.Now);
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = "lad-dev-marketviewer",
                Key = "tickerdetails/stocks.json"
            };
            var s3Response = await amazonS3Client.GetObjectAsync(request, cancellationToken);

            using var streamReader = new StreamReader(s3Response.ResponseStream);
            var json = await streamReader.ReadToEndAsync(cancellationToken);

            var tickerDetailsList = JsonSerializer.Deserialize<IEnumerable<TickerDetails>>(json);
            var tickers = tickerDetailsList.Select(tickerDetails => tickerDetails.Ticker);

            _marketCache.SetTickers(tickers);

            foreach (var tickerDetails in tickerDetailsList)
            {
                _marketCache.SetTickerDetails(tickerDetails);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Error getting ticker details: {ex.Message}");
        }
        sp.Stop();
        logger.LogInformation("TickerDetails - Finished populating at: {time}.", DateTimeOffset.Now);
        logger.LogInformation("TickerDetails - Time elapsed: {elapsed}ms.", sp.ElapsedMilliseconds);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Start");
        return Task.CompletedTask;
    }

    public Task StartedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Started");
        return Task.CompletedTask;
    }

    public Task StoppingAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stop");
        return Task.CompletedTask;
    }

    public Task StoppedAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopped");
        return Task.CompletedTask;
    }
}
