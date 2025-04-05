using AutoFixture;
using Xunit;
using FluentAssertions;
using Polygon.Client.Models;
using System.Text.Json;
using MarketViewer.Contracts.Enums;
using FluentAssertions.Common;
using Moq.AutoMock;
using MarketViewer.Studies.Studies;
using MarketViewer.Contracts.Presentation.Responses;

namespace MarketViewer.Studies.UnitTests;

public class RSIUnitTests
{
    private readonly StudyFactory _classUnderTest;
    private readonly IFixture _autoFixture = new Fixture();
    private readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public RSIUnitTests()
    {
        _classUnderTest = new StudyFactory(new SMA(), new EMA(), new MACD(), new RSI(), new VWAP(), new RVOL(null));
    }

    [Fact]
    public void RSI_With_No_Parameters_Returns_Null()
    {
        // Arrange 
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = _classUnderTest.Compute(StudyType.rsi, null, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void RSI_With_Invalid_Parameters_Returns_Null()
    {
        // Arrange 
        string[] parameters = ["asdf"];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = _classUnderTest.Compute(StudyType.rsi, null, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void RSI_With_No_Candles_Returns_Null()
    {
        // Arrange
        string[] parameters = ["14", "70", "30", "EMA"];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = _classUnderTest.Compute(StudyType.rsi, null, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Theory]
    [InlineData("SMA", 22.38f)]
    [InlineData("EMA", 16.73f)]
    //[InlineData("Wilders", 64.82f)]
    public void RSI_Calculation_Is_Valid(string type, float expectedValue)
    {
        // Arrange
        var json = File.OpenText("./Data/minute.json").ReadToEnd();
        var stocksResponse = JsonSerializer.Deserialize<StocksResponse>(json, _options);

        string[] parameters = ["14", "70", "30", type];

        var dateTime = new DateTime(2025, 2, 26, 12, 0, 0);
        var offset = TimeZone.IsDaylightSavingTime(dateTime) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
        var timestamp = dateTime.ToDateTimeOffset(offset).ToUnixTimeMilliseconds();

        var response = _classUnderTest.Compute(StudyType.rsi, parameters, stocksResponse);

        // Assert
        response.Results.Should().NotBeNull();

        var line = response.Results.First();
        var candle = line.Single(q => q.Timestamp == timestamp);
        candle.Value.Should().BeApproximately(expectedValue, .01f);
    }
}
