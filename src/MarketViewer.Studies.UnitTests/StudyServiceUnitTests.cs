using AutoFixture;
using Xunit;
using FluentAssertions;
using Moq.AutoMock;
using MarketViewer.Contracts.Enums;
using Polygon.Client.Models;

namespace MarketViewer.Studies.UnitTests.Services
{
    public class StudyServiceUnitTests
    {
        #region Private Fields
        private Fixture _autoFixture;
        private AutoMocker _autoMocker;
        #endregion

        #region Constructor
        public StudyServiceUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();
        }
        #endregion

        [Fact]
        public void Compute_EMA()
        {
            // Arrange
            var type = StudyType.ema;
            string[] parameters = ["9"];

            var candles = _autoFixture.CreateMany<Bar>(500).ToArray();

            // Act
            var response = StudyService.ComputeStudy(type, parameters, candles);

            // Assert
            response.Name.Should().Be(type.ToString().ToUpperInvariant());
            response.Parameters.Should().BeEquivalentTo(parameters);
            response.Results.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Compute_SMA()
        {
            // Arrange
            var type = StudyType.sma;
            string[] parameters = ["9"];

            var candles = _autoFixture.CreateMany<Bar>(500).ToArray();

            // Act
            var response = StudyService.ComputeStudy(type, parameters, candles);

            // Assert
            response.Name.Should().Be(type.ToString().ToUpperInvariant());
            response.Parameters.Should().BeEquivalentTo(parameters);
            response.Results.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Compute_MACD()
        {
            // Arrange
            var type = StudyType.macd;
            string[] parameters = ["12","26","9","EMA"];

            var candles = _autoFixture.CreateMany<Bar>(500).ToArray();

            // Act
            var response = StudyService.ComputeStudy(type, parameters, candles);

            // Assert
            response.Name.Should().Be(type.ToString().ToUpperInvariant());
            response.Parameters.Should().BeEquivalentTo(parameters);
            response.Results.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Compute_RSI()
        {
            // Arrange
            var type = StudyType.rsi;

            var candles = _autoFixture.CreateMany<Bar>(500).ToArray();

            // Act
            var response = StudyService.ComputeStudy(type, null, candles);

            // Assert
            response.Name.Should().Be(type.ToString().ToUpperInvariant());
            response.Parameters.Should().BeNullOrEmpty();
            response.Results.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Compute_VWAP()
        {
            // Arrange
            var type = StudyType.vwap;

            var candles = _autoFixture.CreateMany<Bar>(500).ToArray();

            // Act
            var response = StudyService.ComputeStudy(type, null, candles);

            // Assert
            response.Name.Should().Be(type.ToString().ToUpperInvariant());
            response.Parameters.Should().BeNullOrEmpty();
            response.Results.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Compute_Default()
        {
            // Arrange
            var type = StudyType.sma;
            string[] parameters = ["9"];

            var candles = new List<Bar>().ToArray();

            // Act
            var response = StudyService.ComputeStudy(type, parameters, candles);

            // Assert
            response.Should().BeNull();
        }
    }
}
