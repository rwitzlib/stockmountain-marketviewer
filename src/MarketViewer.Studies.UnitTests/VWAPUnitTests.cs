using AutoFixture;
using Xunit;
using FluentAssertions;
using Polygon.Client.Models;
using MarketViewer.Studies.Studies;
using MarketViewer.Contracts.Enums;
using System.Text.Json;
using FluentAssertions.Common;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Studies.UnitTests;

public class VWAPUnitTests    

{
    private readonly StudyFactory _classUnderTest;
    private readonly IFixture _autoFixture = new Fixture();
    private readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public VWAPUnitTests()
    {
        _classUnderTest = new StudyFactory(new SMA(), new EMA(), new MACD(), new RSI(), new VWAP(), new RVOL(null));
    }

    [Fact]
    public void VWAP_With_Parameters_Returns_Null()
    {
        // Arrange 
        string[] parameters = ["asdf"];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = _classUnderTest.Compute(StudyType.vwap, parameters, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void VWAP_Returns_Valid_Response()
    {
        // Arrange 
        var json = File.OpenText("./Data/minute.json").ReadToEnd();
        var stocksResponse = JsonSerializer.Deserialize<StocksResponse>(json, _options);

        var dateTime = new DateTime(2025, 2, 26, 12, 0, 0);
        var offset = TimeZone.IsDaylightSavingTime(dateTime) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
        var timestamp = dateTime.ToDateTimeOffset(offset).ToUnixTimeMilliseconds();


        // Act
        var response = _classUnderTest.Compute(StudyType.vwap, null, stocksResponse);

        // Assert
        response.Results.Should().NotBeNull();

        var line = response.Results.First();
        line.Single(q => q.Timestamp == timestamp).Value.Should().BeApproximately(301.99f, .25f);
    }

    [Fact]
    public void VWAP_Intraday_Returns_Valid_Response()
    {
        // Arrange 
        var json = File.OpenText("./Data/minute.json").ReadToEnd();
        var stocksResponse = JsonSerializer.Deserialize<StocksResponse>(json, _options);

        var dateTime = new DateTime(2025, 2, 26, 12, 0, 0);
        var offset = TimeZone.IsDaylightSavingTime(dateTime) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
        var timestamp = dateTime.ToDateTimeOffset(offset).ToUnixTimeMilliseconds();

        stocksResponse.Results = stocksResponse.Results.Where(q => DateTimeOffset.FromUnixTimeMilliseconds(q.Timestamp).ToOffset(offset).Day == 26).ToList();

        // Act
        var response = _classUnderTest.Compute(StudyType.vwap, null, stocksResponse);

        // Assert
        response.Results.Should().NotBeNull();
        var line = response.Results.First();
        line.Single(q => q.Timestamp == timestamp).Value.Should().BeApproximately(301.99f, .25f);
    }
}
