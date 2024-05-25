using FluentAssertions;
using MarketDataProvider.Contracts.Models;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses;
using MarketViewer.Core.Scanner.Filters;
using MarketViewer.Core.ScanV2.Filters;
using Microsoft.Extensions.Logging.Abstractions;
using Polygon.Client.Models;
using Xunit;

namespace MarketViewer.Core.UnitTests.Scanner
{
    public class PriceFilterUnitTests
    {
        private readonly PriceFilter _classUnderTest;

        public PriceFilterUnitTests()
        {
            _classUnderTest = new PriceFilter(new NullLogger<PriceFilter>());
        }
        
        [Theory]
        [InlineData(1, FilterOperator.gt, 5, true)]
        [InlineData(10, FilterOperator.gt, 5, false)]
        [InlineData(10, FilterOperator.gt, 10, false)]
        [InlineData(5, FilterOperator.lt, 11, false)]
        [InlineData(5, FilterOperator.lt, 1, true)]
        [InlineData(5, FilterOperator.lt, 5, false)]
        public void FilterOnPrice(int filterValue, FilterOperator filterOperator, float candlePrice, bool expected)
        {
            var filter = new Filter
            {
                Type = FilterType.Price,
                Operator = filterOperator,
                Multiplier = 5,
                Value = filterValue
            };

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithClose(candlePrice));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(expected);
        }

        private Bar GenerateCandleWithClose(float close)
        {
            return new Bar { Close = close };
        }
    }
}
