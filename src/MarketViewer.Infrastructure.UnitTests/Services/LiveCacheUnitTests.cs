using Amazon;
using Amazon.S3;
using FluentAssertions;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums;
using MarketViewer.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MarketViewer.Infrastructure.UnitTests.Services;

public class LiveCacheUnitTests
{
    private readonly LiveCache _classUnderTest;
    private readonly MarketCache _marketCache;

    public LiveCacheUnitTests()
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var s3Client = new AmazonS3Client(RegionEndpoint.USEast2);

        _marketCache = new MarketCache(memoryCache, s3Client);
        _classUnderTest = new LiveCache(_marketCache, new NullLogger<LiveCache>());
    }

    [Fact(Skip = "Cant run s3 commands in unit tests")]
    public async Task GetStocksResponsesForDate()
    {
        var timezone = TimeZoneInfo.FindSystemTimeZoneById("CST");

        var offset = timezone.IsDaylightSavingTime(DateTimeOffset.Now) ? TimeSpan.FromHours(-5) : TimeSpan.FromHours(-6);
        var date = new DateTimeOffset(2024, 9, 18, 8, 30, 0, offset);

        List<Timespan> timespans = [
            Timespan.minute,
            Timespan.hour
        ];

        var response = await _classUnderTest.GetStocksResponses(date, timespans);

        _marketCache.GetTickersByTimespan(Timespan.minute, date).Count().Should().Be(5122);
        _marketCache.GetTickersByTimespan(Timespan.hour, date).Count().Should().BeCloseTo(8864, 200);

        response.Responses.TryGetValue(Timespan.minute, out var minuteStocksResponses);
        minuteStocksResponses.Count().Should().Be(4350);

        response.Responses.TryGetValue(Timespan.hour, out var hourStocksResponses);
        hourStocksResponses.Count().Should().BeCloseTo(5220, 200);
    }
}
