using Amazon;
using Amazon.S3;
using MarketViewer.Application.Handlers.Scan;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Requests.Scan;
using MarketViewer.Core.ScanV2;
using MarketViewer.Core.ScanV2.Filters;
using MarketViewer.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MarketViewer.Application.UnitTests.Handlers;

public class ScanHandlerV2UnitTests
{
    private readonly ScanHandlerV2 _classUnderTest;
    private readonly MarketCache _marketCache;
    private readonly LiveCache _liveCache;
    private readonly HistoryCache _historyCache;

    public ScanHandlerV2UnitTests()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var s3Client = new AmazonS3Client(RegionEndpoint.USEast2);

        _marketCache = new MarketCache(memoryCache, s3Client);
        _liveCache = new LiveCache(_marketCache, new NullLogger<LiveCache>());
        _historyCache = new HistoryCache(_marketCache, new NullLogger<HistoryCache>());

        IServiceCollection services = new ServiceCollection();
        services.AddSingleton<ScanFilterFactoryV2>()
            .AddSingleton<PriceActionFilter>()
            .AddSingleton<StudyFilter>()
            .AddSingleton<ValueFilter>();

        var serviceProvider = services.BuildServiceProvider();

        var scanFilterFactory = new ScanFilterFactoryV2(serviceProvider);

        _classUnderTest = new ScanHandlerV2(_liveCache, _historyCache, scanFilterFactory, new NullLogger<ScanHandlerV2>());
    }

    [Fact(Skip = "Timezone works weird in pipeline")]
    public async Task asdf()
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("CST");

        var offset = timezone.IsDaylightSavingTime(DateTimeOffset.Now) ? TimeSpan.FromHours(-5) : TimeSpan.FromHours(-6);
        var date = new DateTimeOffset(2024, 9, 18, 8, 30, 0, offset);

        var json = "{\"Timestamp\":\"2024-09-17T07:49:00-05:00\",\"Argument\":{\"Operator\":\"AND\",\"Filters\":[{\"CollectionModifier\":\"ANY\",\"FirstOperand\":{\"Study\":\"rsi\",\"Modifier\":\"Slope\",\"Multiplier\":1,\"Timespan\":\"hour\"},\"Operator\":\"lt\",\"SecondOperand\":{\"Value\":0},\"Timeframe\":{\"Multiplier\":4,\"Timespan\":\"minute\"}},{\"CollectionModifier\":\"ALL\",\"FirstOperand\":{\"Study\":\"rsi\",\"Modifier\":\"Slope\",\"Multiplier\":1,\"Timespan\":\"hour\"},\"Operator\":\"gt\",\"SecondOperand\":{\"Value\":0},\"Timeframe\":{\"Multiplier\":3,\"Timespan\":\"minute\"}},{\"CollectionModifier\":\"ALL\",\"FirstOperand\":{\"Study\":\"macd\",\"Modifier\":\"Slope\",\"Parameters\":\"12,26,9,ema\",\"Multiplier\":1,\"Timespan\":\"hour\"},\"Operator\":\"gt\",\"SecondOperand\":{\"Value\":0},\"Timeframe\":{\"Multiplier\":3,\"Timespan\":\"minute\"}},{\"CollectionModifier\":\"ALL\",\"FirstOperand\":{\"Study\":\"macd\",\"Modifier\":\"Value\",\"Parameters\":\"12,26,9,ema\",\"Multiplier\":1,\"Timespan\":\"hour\"},\"Operator\":\"lt\",\"SecondOperand\":{\"Value\":0},\"Timeframe\":{\"Multiplier\":2,\"Timespan\":\"minute\"}},{\"CollectionModifier\":\"ALL\",\"FirstOperand\":{\"PriceAction\":\"Volume\",\"Modifier\":\"Value\",\"Multiplier\":1,\"Timespan\":\"minute\"},\"Operator\":\"gt\",\"SecondOperand\":{\"Value\":50000},\"Timeframe\":{\"Multiplier\":5,\"Timespan\":\"minute\"}},{\"CollectionModifier\":\"ALL\",\"FirstOperand\":{\"PriceAction\":\"Vwap\",\"Modifier\":\"Value\",\"Multiplier\":1,\"Timespan\":\"hour\"},\"Operator\":\"gt\",\"SecondOperand\":{\"Value\":2},\"Timeframe\":{\"Multiplier\":5,\"Timespan\":\"minute\"}},{\"CollectionModifier\":\"ALL\",\"FirstOperand\":{\"PriceAction\":\"Vwap\",\"Modifier\":\"Value\",\"Multiplier\":1,\"Timespan\":\"hour\"},\"Operator\":\"lt\",\"SecondOperand\":{\"Value\":25},\"Timeframe\":{\"Multiplier\":5,\"Timespan\":\"minute\"}}]}}\r\n";

        var request = JsonSerializer.Deserialize<ScanV2Request>(json);

        //for (int i = 0; i < 389; i++)
        //{
        //    var response = await _classUnderTest.Handle(request, default);
        //}
    }
}
