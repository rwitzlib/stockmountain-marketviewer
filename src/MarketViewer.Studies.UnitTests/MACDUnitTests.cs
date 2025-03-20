using AutoFixture;
using Moq.AutoMock;
using Xunit;
using FluentAssertions;
using Polygon.Client.Models;
using System.Text.Json;
using MarketViewer.Contracts.Responses;
using MarketViewer.Contracts.Enums;
using FluentAssertions.Common;

namespace MarketViewer.Studies.UnitTests;

public class MACDUnitTests(StudyFixture fixture) : IClassFixture<StudyFixture>
{
    private readonly IFixture _autoFixture = new Fixture();

    private readonly JsonSerializerOptions _options = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public void MACD_With_Null_Parameters_Returns_ErrorMessages()
    {
        // Arrange 
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.macd, null, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void MACD_With_Invalid_Parameters_Returns_ErrorMessages()
    {
        // Arrange 
        string[] parameters = ["12", "26", "9"];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.macd, parameters, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void MACD_With_No_Candles_Returns_ErrorMessages()
    {
        // Arrange 
        string[] parameters = ["12", "26", "9", "EMA"];
        var stocksResponse = new StocksResponse
        {
            Results = []
        };

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.macd, parameters, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void MACD_With_Too_Few_Candles_Returns_ErrorMessages()
    {
        // Arrange 
        string[] parameters = ["12", "26", "9", "EMA"];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(25).ToList()
        };

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.macd, parameters, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Theory]
    [InlineData("12", "26", "9", "WMA")]
    [InlineData("asdf", "26", "9", "EMA")]
    [InlineData("12", "asdf", "9", "EMA")]
    [InlineData("12", "26", "asdf", "EMA")]
    public void MACD_With_Invalid_Parameter_Types_Returns_ErrorMessages(string fast, string slow, string signal, string type)
    {
        // Arrange 
        string[] parameters = [fast, slow, signal, type];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.macd, parameters, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void MACD_EMA_Returns_Valid_Response()
    {
        // Arrange 
        string[] parameters = ["12", "26", "9", "EMA"];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.macd, parameters, stocksResponse);

        // Assert
        response.Results.Should().NotBeNull();
    }

    [Theory]
    [InlineData("12", "26", "9", "EMA", -.334)]
    [InlineData("12", "26", "9", "SMA", -.53)]
    //[InlineData("12", "26", "9", "Wilders", -.133)]
    //[InlineData("12", "26", "9", "Wilders", -.59)]
    //[InlineData("12", "26", "9", "Wilders", -.70)]
    public void MACD_Returns_Correct_Value(string fast, string slow, string signal, string type, float expectedValue)
    {
        // Arrange 
        var json = File.OpenText("./Data/data.json").ReadToEnd();
        var stocksResponse = JsonSerializer.Deserialize<StocksResponse>(json, _options);
        string[] parameters = [fast, slow, signal, type];

        var dateTime = new DateTime(2025, 2, 26, 12, 0, 0);
        var offset = fixture.TimeZone.IsDaylightSavingTime(dateTime) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
        var timestamp = dateTime.ToDateTimeOffset(offset).ToUnixTimeMilliseconds();

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.macd, parameters, stocksResponse);

        // Assert
        response.Results.Should().NotBeNull();

        var line = response.Results.First();
        var candle = line.Single(q => q.Timestamp == timestamp);
        candle.Value.Should().BeApproximately(expectedValue, .01f);
    }
}
