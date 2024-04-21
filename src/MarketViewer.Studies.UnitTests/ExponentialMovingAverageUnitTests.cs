using AutoFixture;
using Moq.AutoMock;
using Xunit;
using FluentAssertions;
using MarketDataProvider.Contracts.Models;
using Polygon.Client.Models;

namespace MarketViewer.Studies.UnitTests
{
    public class ExponentialMovingAverageUnitTests
    {
        private readonly IFixture _autoFixture;
        private readonly AutoMocker _autoMocker;

        public ExponentialMovingAverageUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();
        }

        [Fact]
        public void EMA_With_Invalid_Parameters_Returns_ErrorMessages()
        {
            // Arrange 
            string[] parameters = new[] { "asdf" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = ExponentialMovingAverage.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void EMA_With_Too_Many_Parameters_Returns_ErrorMessages()
        {
            // Arrange 
            string[] parameters = new[] { "9", "9" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = ExponentialMovingAverage.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void EMA_With_No_Parameters_Returns_ErrorMessages()
        {
            // Arrange 
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = ExponentialMovingAverage.Compute(candles, null);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void EMA_Returns_Valid_Response()
        {
            // Arrange
            string[] parameters = new[] { "9" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = ExponentialMovingAverage.Compute(candles, parameters);

            // Assert
            response.Lines.Should().NotBeNull();
            response.ErrorMessages.Should().BeNullOrEmpty();
        }

        [Fact]
        public void EMA_With_Too_High_Weight_Returns_ErrorMessages()
        {
            // Arrange
            string[] parameters = new[] { "101" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = ExponentialMovingAverage.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }
    }
}
