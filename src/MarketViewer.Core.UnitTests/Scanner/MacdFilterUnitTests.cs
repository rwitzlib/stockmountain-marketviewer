using FluentAssertions;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Responses;
using MarketViewer.Core.Scanner.Filters;
using Microsoft.Extensions.Logging.Abstractions;
using Polygon.Client.Models;
using Xunit;

namespace MarketViewer.Core.UnitTests.Scanner
{
    public class MacdFilterUnitTests
    {
        private readonly MacdFilter _classUnderTest;

        public MacdFilterUnitTests()
        {
            _classUnderTest = new MacdFilter(new NullLogger<MacdFilter>());
        }

        #region Greater Than
        [Fact]
        public void GT_MACD_Positive_Slope_Is_Pass()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.gt);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: i, vwap: i, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void GT_MACD_Zero_Slope_Is_Fail()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.gt);

            var results = new List<Bar>();

            for (int i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: 5, vwap: 5, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }

        [Fact]
        public void GT_MACD_Negative_Slope_Is_Fail()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.gt);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: -i, vwap: -i, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }
        #endregion

        #region Greater Than or Equal to
        [Fact]
        public void GE_MACD_Positive_Slope_Is_Pass()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.ge);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: i, vwap: i, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void GE_MACD_Zero_Slope_Is_Pass()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.ge);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: i, vwap: i, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void GE_MACD_Negative_Slope_Is_Fail()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.ge);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: -i, vwap: -i, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }
        #endregion

        #region Less Than
        [Fact]
        public void LT_MACD_Positive_Slope_Is_Fail()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.lt);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: i, vwap: i, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }

        [Fact]
        public void LT_MACD_Zero_Slope_Is_Fail()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.lt);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: i, vwap: i, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }

        [Fact]
        public void LT_MACD_Negative_Slope_Is_Pass()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.lt);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: -i, vwap: -i, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }
        #endregion

        #region Less Than or Equal to
        [Fact]
        public void LE_MACD_Positive_Slope_Is_Pass()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.le);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: i, vwap: i, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }

        [Fact]
        public void LE_MACD_Zero_Slope_Is_Pass()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.le);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: 2, vwap: 2, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void LE_MACD_Negative_Slope_Is_Fail()
        {
            var filter = GivenMacdSlopeFilterWithOperator(FilterOperator.le);

            var results = new List<Bar>();

            for (var i = 0; i < 60; i++)
            {
                results.Add(GenerateCandleWithIncreasingCloseAndVwap(close: -i, vwap: -i, multiplier: i, timeIncrement: i));
            }

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }
        #endregion

        private static Filter GivenMacdSlopeFilterWithOperator(FilterOperator filterOperator)
        {
            return new Filter
            {
                Type = FilterType.Macd,
                Modifier = FilterTypeModifier.Slope,
                Operator = filterOperator,
                Value = 10,
                Multiplier = 5,
                Timespan = Timespan.minute
            };
        }

        private static Bar GenerateCandleWithIncreasingCloseAndVwap(float close, float vwap, float multiplier, float timeIncrement)
        {
            return new Bar { Close = close * multiplier, Vwap = vwap * multiplier, Timestamp = DateTimeOffset.Now.AddMinutes(timeIncrement).ToUnixTimeMilliseconds() };
        }
    }
}
