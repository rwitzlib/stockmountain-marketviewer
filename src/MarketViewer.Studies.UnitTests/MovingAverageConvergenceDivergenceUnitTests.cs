using AutoFixture;
using MarketViewer.Contracts.Models;
using Moq.AutoMock;
using Xunit;
using FluentAssertions;
using MarketDataProvider.Contracts.Models;
using Polygon.Client.Models;

namespace MarketViewer.Studies.UnitTests
{
    public class MovingAverageConvergenceDivergenceUnitTests
    {
        private readonly IFixture _autoFixture;
        private readonly AutoMocker _autoMocker;

        public MovingAverageConvergenceDivergenceUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();
        }

        [Fact]
        public void MACD_With_Invalid_Parameters_Returns_ErrorMessages()
        {
            // Arrange 
            string[] parameters = new[] { "12", "26", "9" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void MACD_With_Null_Parameters_Returns_ErrorMessages()
        {
            // Arrange 
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, null);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void MACD_EMA_Returns_Valid_Response()
        {
            // Arrange 
            string[] parameters = new[] { "12", "26", "9", "EMA" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().NotBeNull();
            response.ErrorMessages.Should().BeNullOrEmpty();
        }

        [Fact]
        public void MACD_SMA_Returns_Valid_Response()
        {
            // Arrange 
            string[] parameters = new[] { "12", "26", "9", "SMA" };
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().NotBeNull();
            response.ErrorMessages.Should().BeNullOrEmpty();
        }

        [Fact]
        public void MACD_With_No_Candles_Returns_ErrorMessages()
        {
            // Arrange 
            string[] parameters = new[] { "12", "26", "9", "EMA" };
            var candles = new Bar[] { };

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void MACD_With_Too_Few_Candles_Returns_ErrorMessages()
        {
            // Arrange 
            string[] parameters = new[] { "12", "26", "9", "EMA" };
            var candles = _autoFixture.CreateMany<Bar>(5).ToArray();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("12", "26", "9", "WMA")]
        [InlineData("asdf", "26", "9", "EMA")]
        [InlineData("12", "asdf", "9", "EMA")]
        [InlineData("12", "26", "asdf", "EMA")]
        public void MACD_With_Invalid_Parameter_Types_Returns_ErrorMessages(string fast, string slow, string signal, string type)
        {
            // Arrange 
            string[] parameters = new[] { fast, slow, signal, type };
            var candles = _autoFixture.CreateMany<Bar>(100).ToArray();

            // Act
            var response = MovingAverageConvergenceDivergence.Compute(candles, parameters);

            // Assert
            response.Lines.Should().BeNull();
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }
    }
}
