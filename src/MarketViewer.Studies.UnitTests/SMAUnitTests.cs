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
using MarketViewer.Contracts.Responses;
using System.Text.Json;
using MarketViewer.Studies.Studies;
using MarketViewer.Contracts.Enums;
using FluentAssertions.Common;

namespace MarketViewer.Studies.UnitTests
{
    public class SMAUnitTests(StudyFixture fixture) : IClassFixture<StudyFixture>
    {
        private readonly IFixture _autoFixture = new Fixture();
        private readonly AutoMocker _autoMocker;

        private readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        [Fact]
        public void SMA_With_No_Parameters_Returns_Null()
        {
            // Arrange 
            var stocksResponse = new StocksResponse
            {
                Results = _autoFixture.CreateMany<Bar>(100).ToList()
            };

            // Act
            var response = fixture.StudyFactory.Compute(StudyType.sma, null, stocksResponse);

            // Assert
            response.Should().BeNull();
        }
        
        [Fact]
        public void SMA_With_Invalid_Parameters_Returns_Null()
        {
            // Arrange 
            string[] parameters = ["asdf"];
            var stocksResponse = new StocksResponse
            {
                Results = _autoFixture.CreateMany<Bar>(100).ToList()
            };

            // Act
            var response = fixture.StudyFactory.Compute(StudyType.sma, parameters, stocksResponse);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void SMA_With_Too_Many_Parameters_Returns_Null()
        {
            // Arrange 
            string[] parameters = ["9", "9"];
            var stocksResponse = new StocksResponse
            {
                Results = _autoFixture.CreateMany<Bar>(100).ToList()
            };

            // Act
            var response = fixture.StudyFactory.Compute(StudyType.sma, parameters, stocksResponse);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void SMA_With_Too_High_Or_Low_Weight_Returns_Null()
        {
            // Arrange
            string[] parameters = ["1", "1000"];
            var stocksResponse = new StocksResponse
            {
                Results = _autoFixture.CreateMany<Bar>(100).ToList()
            };

            // Act
            var response = fixture.StudyFactory.Compute(StudyType.sma, parameters, stocksResponse);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void SMA_With_No_Candles_Returns_Null()
        {
            // Arrange
            string[] parameters = ["9"];
            var stocksResponse = new StocksResponse
            {
                Results = _autoFixture.CreateMany<Bar>(8).ToList()
            };

            // Act
            var response = fixture.StudyFactory.Compute(StudyType.sma, parameters, stocksResponse);

            // Assert
            response.Should().BeNull();
        }

        [Fact]
        public void SMA_Returns_Valid_Response()
        {
            // Arrange
            var json = File.OpenText("./Data/data.json").ReadToEnd();
            var stocksResponse = JsonSerializer.Deserialize<StocksResponse>(json, _options);

            string[] parameters = ["9"];

            var dateTime = new DateTime(2025, 2, 26, 12, 0, 0);
            var offset = fixture.TimeZone.IsDaylightSavingTime(dateTime) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
            var timestamp = dateTime.ToDateTimeOffset(offset).ToUnixTimeMilliseconds();

            // Act
            var response = fixture.StudyFactory.Compute(StudyType.sma, parameters, stocksResponse);

            // Assert
            response.Results.Should().NotBeNull();

            var line = response.Results.First();
            line.Single(q => q.Timestamp == timestamp).Value.Should().BeApproximately(299.41f, .01f);
        }
    }
}
