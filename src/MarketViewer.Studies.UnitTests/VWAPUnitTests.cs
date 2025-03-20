using AutoFixture;
using MarketViewer.Contracts.Models;
using Moq.AutoMock;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using MarketDataProvider.Contracts.Models;
using Polygon.Client.Models;
using MarketViewer.Studies.Studies;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Responses;
using System.Text.Json;
using FluentAssertions.Common;

namespace MarketViewer.Studies.UnitTests;

public class VWAPUnitTests(StudyFixture fixture) : IClassFixture<StudyFixture>
{
    private readonly IFixture _autoFixture = new Fixture();
    private readonly AutoMocker _autoMocker = new AutoMocker();

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

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
        var response = fixture.StudyFactory.Compute(StudyType.vwap, parameters, stocksResponse);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public void VWAP_Returns_Valid_Response()
    {
        // Arrange 
        var json = File.OpenText("./Data/data.json").ReadToEnd();
        var stocksResponse = JsonSerializer.Deserialize<StocksResponse>(json, _options);

        var dateTime = new DateTime(2025, 2, 26, 12, 0, 0);
        var offset = fixture.TimeZone.IsDaylightSavingTime(dateTime) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
        var timestamp = dateTime.ToDateTimeOffset(offset).ToUnixTimeMilliseconds();


        // Act
        var response = fixture.StudyFactory.Compute(StudyType.vwap, null, stocksResponse);

        // Assert
        response.Results.Should().NotBeNull();

        var line = response.Results.First();
        line.Single(q => q.Timestamp == timestamp).Value.Should().BeApproximately(301.99f, .25f);
    }

    [Fact]
    public void VWAP_Intraday_Returns_Valid_Response()
    {
        // Arrange 
        var json = File.OpenText("./Data/data.json").ReadToEnd();
        var stocksResponse = JsonSerializer.Deserialize<StocksResponse>(json, _options);

        var dateTime = new DateTime(2025, 2, 26, 12, 0, 0);
        var offset = fixture.TimeZone.IsDaylightSavingTime(dateTime) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
        var timestamp = dateTime.ToDateTimeOffset(offset).ToUnixTimeMilliseconds();

        stocksResponse.Results = stocksResponse.Results.Where(q => DateTimeOffset.FromUnixTimeMilliseconds(q.Timestamp).ToOffset(offset).Day == 26).ToList();

        // Act
        var response = fixture.StudyFactory.Compute(StudyType.vwap, null, stocksResponse);

        // Assert
        response.Results.Should().NotBeNull();
        var line = response.Results.First();
        line.Single(q => q.Timestamp == timestamp).Value.Should().BeApproximately(301.99f, .25f);
    }
}
