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

namespace MarketViewer.Studies.UnitTests
{
    public class VolumeWeightedAveragePriceUnitTests
    {
        private readonly IFixture _autoFixture;
        private readonly AutoMocker _autoMocker;

        public VolumeWeightedAveragePriceUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();
        }

        [Fact]
        public void VWAP_With_Parameters_Returns_ErrorMessages()
        {
            // Arrange 
            string[] parameters = new[] { "asdf" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = VolumeWeightedAveragePrice.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void VWAP_Returns_Valid_Response()
        {
            // Arrange 
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = VolumeWeightedAveragePrice.Compute(candles, null);

            // Assert
            response.Lines.Should().NotBeNull();
            response.ErrorMessages.Should().BeNullOrEmpty();
        }

        [Fact]
        public void VWAP_Interday_Returns_Valid_Response()
        {
            // Arrange 
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            for(int i = 0; i < candles.Count(); i++ )
            {
                candles.ToArray()[i].Timestamp = DateTimeOffset.Now.AddDays(i).ToUnixTimeMilliseconds();
            }

            // Act
            var response = VolumeWeightedAveragePrice.Compute(candles, null);

            // Assert
            response.Lines.Should().NotBeNull();
            response.ErrorMessages.Should().BeNullOrEmpty();
        }
    }
}
