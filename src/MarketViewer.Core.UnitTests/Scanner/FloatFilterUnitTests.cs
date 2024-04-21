using FluentAssertions;
using MarketDataProvider.Contracts.Models;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses;
using MarketViewer.Core.Scanner;
using MarketViewer.Core.Scanner.Filters;
using Xunit;

namespace MarketViewer.Core.UnitTests.Scanner
{
    public class FloatFilterUnitTests
    {
        //[Theory]
        //[InlineData(1, FilterOperator.gt, 5, true)]
        //[InlineData(10, FilterOperator.gt, 5, false)]
        //[InlineData(5, FilterOperator.gt, 5, true)]
        //[InlineData(5, FilterOperator.lt, 11, false)]
        //[InlineData(5, FilterOperator.lt, 1, true)]
        //[InlineData(5, FilterOperator.lt, 5, true)]
        //public void FilterOnFloat(int filterValue, FilterOperator filterOperator, float floatValue, bool expected)
        //{
        //    var filter = new Filter
        //    {
        //        Type = FilterType.Float,
        //        Operator = filterOperator,
        //        Multiplier = 5,
        //        Value = filterValue
        //    };

        //    var aggregateResponse = new StocksResponse
        //    {
        //        TickerDetails = new TickerDetails
        //        {
        //            Float = (long)floatValue
        //        }
        //    };

        //    var scanFilter = new FloatFilter(filter);
        //    var response = scanFilter.ApplyFilter(aggregateResponse);

        //    response.Should().Be(expected);
        //}
    }
}
