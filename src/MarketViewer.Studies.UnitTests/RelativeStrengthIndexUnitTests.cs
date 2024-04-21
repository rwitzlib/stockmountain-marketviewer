using AutoFixture;
using MarketViewer.Contracts.Models;
using Moq.AutoMock;
using Xunit;
using FluentAssertions;
using MarketDataProvider.Contracts.Models;
using Polygon.Client.Models;

namespace MarketViewer.Studies.UnitTests
{
    public class RelativeStrengthIndexUnitTests
    {
        private readonly IFixture _autoFixture;
        private readonly AutoMocker _autoMocker;

        public RelativeStrengthIndexUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();
        }

        [Fact]
        public void RSI_With_Invalid_Parameters_Returns_ErrorMessages()
        {
            // Arrange 
            string[] parameters = new[] { "asdf" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = RelativeStrengthIndex.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RSI_Returns_Valid_Response()
        {
            // Arrange 
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = SimpleMovingAverage.Compute(candles, null);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void RSI_With_No_Candles_Returns_ErrorMessages()
        {
            // Arrange
            var candles = new Bar[] { };

            // Act
            var response = SimpleMovingAverage.Compute(candles, null);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }
    }
}
