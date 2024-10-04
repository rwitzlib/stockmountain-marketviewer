using AutoFixture;
using Moq;
using Xunit;
using FluentAssertions;
using MarketViewer.Contracts.Responses;
using MarketViewer.Application.Handlers;
using MarketViewer.Contracts.Enums;
using MarketViewer.Infrastructure.Services;
using Moq.AutoMock;
using System.Text.Json;
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using MarketViewer.Core.Scanner;
using MarketViewer.Core.ScanV2;
using MarketViewer.Contracts.Requests.Scan;
using MarketViewer.Contracts.Models.ScanV2;

namespace MarketViewer.Application.UnitTests.Handlers
{
    public class ScanHandlerUnitTests
    {
        private ScanHandler _classUnderTest;
        private Fixture _autoFixture;
        private AutoMocker _autoMocker;
        private Mock<LiveCache> _liveCache;
        private Mock<HistoryCache> _backtestingCache;
        private Mock<ScanFilterFactory> _scanFilterFactory;

        public ScanHandlerUnitTests()
        {
            _autoFixture = new Fixture();
            _autoMocker = new AutoMocker();

            _liveCache = _autoMocker.GetMock<LiveCache>();
            _backtestingCache = _autoMocker.GetMock<HistoryCache>();
            _scanFilterFactory = _autoMocker.GetMock<ScanFilterFactory>();

            _classUnderTest = new ScanHandler(_liveCache.Object, _backtestingCache.Object, _scanFilterFactory.Object, new NullLogger<ScanHandler>());
        }

        //[Fact]
        public async Task BacktestingCache_Returns_Success_Response()
        {
            // Arrange
            var request = new ScanRequest
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Type = FilterType.Volume,
                        Operator = FilterOperator.gt,
                        Value = 20000,
                        Multiplier = 5,
                        Timespan = Timespan.minute
                    }
                },
                Timestamp = DateTime.Now.AddDays(-2)
            };

            await GivenCacheReturnsAggregates();

            // Act
            var response = await _classUnderTest.Handle(request, default);

            // Assert
            response.Status.Should().Be(HttpStatusCode.OK);
            response.Data.Items.Should().NotBeNullOrEmpty();
        }

        //[Fact]
        public async Task LiveCache_Returns_Success_Response()
        {
            // Arrange
            var request = GivenValidRequest();
            request.Timestamp = DateTime.Now;

            // Act
            var response = await _classUnderTest.Handle(request, default);

            // Assert
            response.Status.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Request_With_Null_Filters_Returns()
        {
            // Arrange
            var request = _autoFixture.Create<ScanRequest>();
            request.Filters = null;

            // Act
            var response = await _classUnderTest.Handle(request, default);

            // Assert
            response.Status.Should().Be(HttpStatusCode.BadRequest);
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Request_With_No_Filters_Returns()
        {
            // Arrange
            var request = _autoFixture.Create<ScanRequest>();
            request.Filters = Enumerable.Empty<Filter>();

            // Act
            var response = await _classUnderTest.Handle(request, default);

            // Assert
            response.Status.Should().Be(HttpStatusCode.BadRequest);
            response.ErrorMessages.Should().NotBeNullOrEmpty();
        }

        //[Fact]
        public async Task Cache_Returns_No_Aggregates_Returns_Valid_Response()
        {
            // Arrange
            var request = _autoFixture.Create<ScanRequest>();

            // Act
            var response = await _classUnderTest.Handle(request, default);

            // Assert
            response.Status.Should().Be(HttpStatusCode.OK);
            response.Data.Should().NotBeNull();
            response.Data.Items.Should().NotBeNull();
            response.Data.Items.Should().BeEmpty();
        }

        //[Theory]
        [InlineData(FilterType.Volume)]
        [InlineData(FilterType.Macd, FilterTypeModifier.Value)]
        [InlineData(FilterType.Macd, FilterTypeModifier.Slope)]
        [InlineData(FilterType.Price, FilterTypeModifier.Value, FilterValueType.Vwap)]
        [InlineData(FilterType.Vwap)]
        [InlineData(FilterType.Float)]
        public async Task Requests_With_Various_Types_Returns_Empty_Response(FilterType type, FilterTypeModifier? modifier = null, FilterValueType? filterValueType = null)
        {
            // Arrange
            var request = new ScanRequest
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Type = type,
                        Operator = FilterOperator.gt,
                        Value = 5,
                        Timespan = Timespan.minute,
                        Multiplier = 1
                    }
                }
            };

            if (modifier.HasValue)
            {
                request.Filters.First().Modifier = modifier.Value;
            }

            if (filterValueType.HasValue)
            {
                request.Filters.First().ValueType = filterValueType.Value;
            }

            await GivenCacheReturnsAggregates();

            // Act
            var response = await _classUnderTest.Handle(request, default);

            // Assert
            response.Status.Should().Be(HttpStatusCode.OK);
        }

        private ScanRequest GivenValidRequest()
        {
            return new ScanRequest
            {
                Filters = _autoFixture.CreateMany<Filter>(100),
                Timestamp = DateTimeOffset.Now
            };
        }

        private async Task GivenCacheReturnsAggregates()
        {
            var filename = "./backtest.json";
            var stream = File.OpenRead(filename);

            var reader = new StreamReader(stream);
            var json = await reader.ReadToEndAsync();
            var aggregates = JsonSerializer.Deserialize<List<StocksResponse>>(json);

            _backtestingCache.Setup(q => q.GetStocksResponses(It.IsAny<DateTimeOffset>()))
                .ReturnsAsync(aggregates);
        }
    }
}
