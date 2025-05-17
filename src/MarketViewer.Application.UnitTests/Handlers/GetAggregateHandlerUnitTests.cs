using AutoFixture;
using AutoMapper;
using Moq;
using Xunit;
using FluentAssertions;
using MarketViewer.Application.Handlers;
using MarketViewer.Contracts.Interfaces;
using MarketViewer.Contracts.Enums;
using Polygon.Client.Models;
using MarketViewer.Studies;
using MarketViewer.Studies.UnitTests;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;
using MarketViewer.Contracts.Models.Study;

namespace MarketViewer.Application.UnitTests.Handlers
{
    public class GetAggregateHandlerUnitTests : IClassFixture<StudyFixture>
    {
        #region Private Fields
        private StocksHandler _classUnderTest;
        private Fixture _fixture;
        private Mock<IMarketDataRepository> _repository;
        #endregion

        #region Constructor
        public GetAggregateHandlerUnitTests(StudyFixture fixture)
        {
            _fixture = new Fixture();

            _repository = new Mock<IMarketDataRepository>();
            _classUnderTest = new StocksHandler(_repository.Object, fixture.StudyFactory);
        }
        #endregion

        #region Tests
        [Fact]
        public async Task Valid_GetAggregate_Request_Returns_SuccessResponse()
        {
            // Arrange
            var request = GivenAggregateRequestWithNoStudies();
            var response = GivenSuccessfulResponse();

            _repository.Setup(q => q.GetStockDataAsync(It.IsAny<StocksRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.Data.Results.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAggregate_With_Null_Response_From_Repository_Is_Failure()
        {
            // Arrange
            var request = GivenAggregateRequestWithNoStudies();
            var response = GivenSuccessfulResponse();

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.ErrorMessages.Should().Contain("Query returned invalid result.");
        }

        [Fact]
        public async Task GetAggregate_Response_With_Non_OK_Status_Is_Success()
        {
            // Arrange
            var request = GivenAggregateRequestWithNoStudies();
            var response = GivenSuccessfulResponse();

            _repository.Setup(q => q.GetStockDataAsync(It.IsAny<StocksRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.Data.Results.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAggregate_Response_With_No_Results_Is_Failure()
        {
            // Arrange
            var request = GivenAggregateRequestWithNoStudies();
            var response = new StocksResponse
            {
                Ticker = _fixture.Create<string>(),
                Status = _fixture.Create<string>(),
            };

            _repository.Setup(q => q.GetStockDataAsync(It.IsAny<StocksRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.ErrorMessages.Should().Contain("Query returned no results.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetAggregate_With_Invalid_Ticker_Request_Is_Failure(string ticker)
        {
            // Arrange
            var request = GivenAggregateRequestWithNoStudies();
            request.Ticker = ticker;

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.ErrorMessages.Should().Contain("Must include a valid Ticker.");
        }

        [Fact]
        public async Task GetAggregate_With_Too_Early_From_Date_Is_Failure()
        {
            // Arrange
            var request = GivenAggregateRequestWithNoStudies();
            request.From = DateTimeOffset.UnixEpoch.AddDays(-1);

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.ErrorMessages.Should().Contain($"'From' date must be later than {DateTimeOffset.UnixEpoch:yyyy-MM-dd}.");
        }

        [Fact]
        public async Task GetAggregate_With_Invalid_From_Date_Request_Is_Failure()
        {
            // Arrange
            var request = GivenAggregateRequestWithNoStudies();
            request.From = DateTimeOffset.Now.AddDays(1);

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.ErrorMessages.Should().Contain($"'From' date must be earlier than {DateTimeOffset.Now:yyyy-MM-dd}.");
        }

        [Fact]
        public async Task GetAggregate_With_Invalid_From_And_To_Date_Request_Is_Failure()
        {
            // Arrange
            var request = GivenAggregateRequestWithNoStudies();
            request.From = DateTimeOffset.Now.AddDays(-1);
            request.To = DateTimeOffset.Now.AddDays(-2);

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.ErrorMessages.Should().Contain("'From' date must be earlier than 'To' date.");
        }

        [Fact]
        public async Task GetAggregateAsync_With_Invalid_Ticker_And_From_Date_Request_Is_Failure()
        {
            // Arrange
            var request = GivenAggregateRequestWithStudies();
            request.Ticker = string.Empty;
            request.From = DateTimeOffset.Now.AddDays(1);

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.ErrorMessages.Should().Contain("Must include a valid Ticker.");
            result.ErrorMessages.Should().Contain($"'From' date must be earlier than {DateTimeOffset.Now:yyyy-MM-dd}.");
        }

        [Fact]
        public async Task GetAggregate_With_Valid_Study_Should_Return_Response_With_Study()
        {
            // Arrange
            var request = GivenAggregateRequestWithStudies();
            var response = GivenSuccessfulResponse();

            _repository.Setup(q => q.GetStockDataAsync(It.IsAny<StocksRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.Data.Results.Should().NotBeNullOrEmpty();
            result.Data.Studies.First().Name.Should().NotBeNullOrEmpty();
            result.Data.Studies.First().Parameters.Should().NotBeNullOrEmpty();
            result.Data.Studies.First().Results.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAggregate_With_Invalid_Study_Parameters_Should_Return_Response_With_No_Studies()
        {
            // Arrange
            var request = GivenAggregateRequestWithStudies();
            request.Studies.First().Parameters = new[] { "12", "26", "9" };
            var response = GivenSuccessfulResponse();

            _repository.Setup(q => q.GetStockDataAsync(It.IsAny<StocksRequest>()))
                .ReturnsAsync(response);

            // Act
            var result = await _classUnderTest.Handle(request, default);

            // Assert
            result.Data.Results.Should().NotBeNullOrEmpty();
            result.Data.Studies.Should().BeNullOrEmpty();
        }
        #endregion

        #region Private Methods
        private StocksRequest GivenAggregateRequestWithNoStudies()
        {
            var request = new StocksRequest
            {
                Ticker = "AAPL",
                Multiplier = 1,
                Timespan = Timespan.minute,
                From = DateTimeOffset.Now.AddDays(-1),
                To = DateTimeOffset.Now
            };

            return request;
        }

        private StocksRequest GivenAggregateRequestWithStudies()
        {
            var request = new StocksRequest
            {
                Ticker = "AAPL",
                Multiplier = 1,
                Timespan = Timespan.minute,
                From = DateTimeOffset.Now.AddDays(-1),
                To = DateTimeOffset.Now,
                Studies = new List<StudyFields>
                {
                    new StudyFields
                    {
                        Type = StudyType.macd,
                        Parameters = new [] {"12", "26", "9", "EMA"}
                    }
                }
            };

            return request;
        }

        private StocksResponse GivenSuccessfulResponse()
        {
            var response = new StocksResponse
            {
                Ticker = "AAPL",
                Status = "OK",
                Results = _fixture.CreateMany<Bar>(100).ToList()
            };

            return response;
        }
        #endregion
    }
}
