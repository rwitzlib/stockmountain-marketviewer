using AutoFixture;
using Xunit;
using FluentAssertions;
using Polygon.Client.Models;
using System.Text.Json;
using MarketViewer.Contracts.Responses;
using MarketViewer.Contracts.Enums;
using FluentAssertions.Common;

namespace MarketViewer.Studies.UnitTests;

public class EMAUnitTests(StudyFixture fixture) : IClassFixture<StudyFixture>
{
    private readonly IFixture _autoFixture = new Fixture();
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public void EMA_With_No_Parameters_Returns_Null()
    {
        // Arrange 
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.ema, null, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void EMA_With_Invalid_Parameters_Returns_Null()
    {
        // Arrange 
        string[] parameters = ["asdf"];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.ema, parameters, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void EMA_With_Too_Many_Parameters_Returns_Null()
    {
        // Arrange 
        string[] parameters = ["9", "9"];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.ema, parameters, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Theory]
    [InlineData("0")]
    [InlineData("1001")]
    public void EMA_With_High_Or_Low_Weight_Returns_Null(string weight)
    {
        // Arrange
        string[] parameters = [weight];
        var stocksResponse = new StocksResponse
        {
            Results = _autoFixture.CreateMany<Bar>(100).ToList()
        };

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.ema, parameters, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void EMA_Returns_Valid_Response()
    {
        // Arrange
        var json = File.OpenText("./Data/data.json").ReadToEnd();
        var stocksResponse = JsonSerializer.Deserialize<StocksResponse>(json, _options);

        string[] parameters = ["9"];

        var dateTime = new DateTime(2025, 2, 26, 12, 0, 0);
        var offset = fixture.TimeZone.IsDaylightSavingTime(dateTime) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
        var timestamp = dateTime.ToDateTimeOffset(offset).ToUnixTimeMilliseconds();

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.ema, parameters, stocksResponse);

        // Assert
        response.Results.Should().NotBeNull();

        var line = response.Results.First();
        line.Single(q => q.Timestamp == timestamp).Value.Should().BeApproximately(299.21f, .01f);
    }
}
