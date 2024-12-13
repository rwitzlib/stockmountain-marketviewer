using Amazon;
using Amazon.S3;
using MarketViewer.Infrastructure.Mock;
using MarketViewer.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using MarketViewer.Contracts.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using MarketDataProvider.Clients.Interfaces;
using MarketDataProvider.Clients;
using Polygon.Client.DependencyInjection;
using MarketViewer.Contracts.Caching;
using Amazon.DynamoDBv2;

namespace MarketViewer.Infrastructure.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var redisUrl = configuration.GetSection("Urls").GetValue<string>("RedisUrl");
        var marketDataProviderUrl = configuration.GetSection("Urls").GetValue<string>("MarketDataProviderUrl");
        var token = configuration.GetSection("Tokens").GetValue<string>("PolygonApi");

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisUrl);

        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer)
            .AddSingleton<IAmazonS3>(client => new AmazonS3Client(RegionEndpoint.USEast2))
            .AddPolygonClient(token)
            .AddSingleton<LiveCache>()
            .AddSingleton<HistoryCache>()
            .AddSingleton<MarketCache>()
            .AddSingleton<BacktestService>()
            .AddSingleton<IAmazonDynamoDB, AmazonDynamoDBClient>(client => new AmazonDynamoDBClient(RegionEndpoint.USEast2))
            .AddSingleton<IMarketCacheClient, MarketCacheClient>();

        services.AddHttpClient("marketdataprovider", client =>
        {
            client.BaseAddress = new Uri(marketDataProviderUrl);
        });

        //if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Equals("Local"))
        //{
        //    services.AddTransient<IMarketDataRepository, MockAggregateService>();
        //}
        //else
        //{
            services.AddTransient<IMarketDataRepository, MarketDataRepository>();
        //}

        return services;
    }
}
