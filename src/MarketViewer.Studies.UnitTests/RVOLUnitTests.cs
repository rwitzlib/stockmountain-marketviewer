using AutoFixture;
using Moq.AutoMock;
using Xunit;
using FluentAssertions;
using Polygon.Client.Models;
using MarketViewer.Studies.Studies;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using System.Text.Json;
using FluentAssertions.Common;
using MarketViewer.Contracts.Caching;
using Moq;
using MarketViewer.Contracts.Models.Scan;

namespace MarketViewer.Studies.UnitTests;

public class RVOLUnitTests
{
    private readonly StudyFactory _classUnderTest;
    private readonly IFixture _autoFixture = new Fixture();
    private readonly AutoMocker _autoMocker = new AutoMocker();
    private readonly Mock<IMarketCache> _marketCache = new Mock<IMarketCache>();
    private readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public RVOLUnitTests()
    {
        _classUnderTest = new StudyFactory(new SMA(), new EMA(), new MACD(), new RSI(), new VWAP(), new RVOL(_marketCache.Object));
    }

    [Fact]
    public void RVOL_With_Parameters_Returns_Null()
    {
        // Arrange 
        string[] parameters = ["asdf"];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = _classUnderTest.Compute(StudyType.rvol, parameters, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void RVOL_Returns_Valid_Response()
    {
        // Arrange 
        var minuteJson = File.OpenText("./Data/minute.json").ReadToEnd();
        var minuteStocksResponse = JsonSerializer.Deserialize<StocksResponse>(minuteJson, _options);

        var dayJson = File.OpenText("./Data/day.json").ReadToEnd();
        var dayStocksResponse = JsonSerializer.Deserialize<StocksResponse>(dayJson, _options);

        var dateTime = new DateTime(2025, 2, 26, 12, 0, 0);
        var offset = TimeZone.IsDaylightSavingTime(dateTime) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
        var timestamp = dateTime.ToDateTimeOffset(offset).ToUnixTimeMilliseconds();

        _marketCache.Setup(q => q.GetStocksResponse(It.IsAny<string>(), It.IsAny<Timeframe>(), It.IsAny<DateTimeOffset>())).Returns(dayStocksResponse);

        // Act
        var response = _classUnderTest.Compute(StudyType.rvol, null, minuteStocksResponse);

        // Assert
        response.Results.Should().NotBeNull();

        var line = response.Results.First();
        var candle = line.Single(q => q.Timestamp == timestamp);
        candle.Value.Should().BeApproximately(.517f, .01f);
    }
}
