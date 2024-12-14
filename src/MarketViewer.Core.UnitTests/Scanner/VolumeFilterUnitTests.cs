using FluentAssertions;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses;
using MarketViewer.Core.Scan.Filters;
using Microsoft.Extensions.Logging.Abstractions;
using Polygon.Client.Models;
using Xunit;

namespace MarketViewer.Core.UnitTests.Scanner
{
    public class VolumeFilterUnitTests
    {
        private readonly VolumeFilter _classUnderTest;

        public VolumeFilterUnitTests()
        {
            _classUnderTest = new VolumeFilter(new NullLogger<VolumeFilter>());
        }
        
        #region Greater Than
        [Fact]
        public void GT_Volume_Single_Bar_Above_Filter_Value_Is_Pass()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.gt);

            var results = new List<Bar>();
            results.Add(new Bar { Volume = 1 });
            results.Add(new Bar { Volume = 1 });
            results.Add(new Bar { Volume = 1 });
            results.Add(new Bar { Volume = 1 });
            results.Add(new Bar { Volume = 11 });

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void GT_Volume_Multiple_Bar_Above_Filter_Value_Is_Pass()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.gt);

            var results = new List<Bar>
            {
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 6 },
                new Bar { Volume = 7 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void GT_Volume_No_Bar_Above_Filter_Value_Is_Fail()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.gt);

            var results = new List<Bar>
            {
                new Bar { Volume = 2 },
                new Bar { Volume = 2 },
                new Bar { Volume = 2 },
                new Bar { Volume = 2 },
                new Bar { Volume = 2 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }

        [Fact]
        public void GT_Volume_Bar_Count_Less_Than_Filter_Multiplier_Is_Pass()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.gt);

            var results = new List<Bar>
            {
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 6 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void GT_Volume_No_Bar_Within_Multiplier_Range_Is_Fail()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.gt);

            var results = new List<Bar>
            {
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 11 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }

        [Fact]
        public void GT_Bar_Volume_Equal_To_Filter_Value_Is_Fail()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.gt);

            var results = new List<Bar>
            {
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 2 }
            };

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
        public void GE_Bar_Volume_Equal_To_Filter_Value_Is_Pass()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.ge);

            var results = new List<Bar>
            {
                new Bar { Volume = 5 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void GE_Bar_Volume_Above_Filter_Value_Is_Pass()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.ge);

            var results = new List<Bar>
            {
                new Bar { Volume = 6 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void GE_Bar_Volume_Below_Filter_Value_Is_Pass()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.ge);

            var results = new List<Bar>
            {
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }

        [Fact]
        public void GE_No_Bar_Within_Multiplier_Range_Is_Fail()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.ge);

            var results = new List<Bar>
            {
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 }
            };

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
        public void LT_Volume_All_Bars_Below_Filter_Value_Is_Pass()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.lt);

            var results = new List<Bar>
            {
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void LT_Bar_Volume_Equal_To_Filter_Value_Is_Fail()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.lt);

            var results = new List<Bar>
            {
                new Bar { Volume = 5 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }

        [Fact]
        public void LT_Bar_Volume_Above_Filter_Value_Is_Fail()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.lt);

            var results = new List<Bar>
            {
                new Bar { Volume = 6 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }
        #endregion

        #region Less Than or Equal to
        [Fact]
        public void LE_Bar_Volume_Above_Filter_Value_Is_Fail()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.le);

            var results = new List<Bar>
            {
                new Bar { Volume = 6 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 },
                new Bar { Volume = 4 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(false);
        }

        [Fact]
        public void LE_Volume_Bar_Equal_To_Filter_Value_Is_Pass()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.le);

            var results = new List<Bar>
            {
                new Bar { Volume = 2 },
                new Bar { Volume = 2 },
                new Bar { Volume = 2 },
                new Bar { Volume = 2 },
                new Bar { Volume = 2 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }

        [Fact]
        public void LE_Volume_All_Bars_Below_Filter_Value_Is_Pass()
        {
            var filter = GivenVolumeFilterWithOperator(FilterOperator.le);

            var results = new List<Bar>
            {
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 },
                new Bar { Volume = 1 }
            };

            var aggregateResponse = new StocksResponse
            {
                Results = results
            };

            var response = _classUnderTest.ApplyFilter(filter, aggregateResponse);

            response.Should().Be(true);
        }
        #endregion

        private static Filter GivenVolumeFilterWithOperator(FilterOperator filterOperator)
        {
            return new Filter
            {
                Type = FilterType.Volume,
                Operator = filterOperator,
                Value = 10,
                Multiplier = 5,
                Timespan = Timespan.minute
            };
        }
    }
}
