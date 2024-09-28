using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Responses;
using Microsoft.Extensions.Logging;

namespace MarketViewer.Core.Scanner.Filters
{
    public class FloatFilter(ILogger<FloatFilter> logger) : IFilter
    {
        public bool ApplyFilter(Filter filter, StocksResponse response)
        {
            try
            {
                if (filter is null || !response.Results.Any())
                {
                    return false;
                }

                return filter.ValueType switch
                {
                    FilterValueType.CustomAmount => filter.Modifier switch
                    {
                        FilterTypeModifier.Value => FilterByValue(filter, response),
                        _ => false
                    },
                    _ => false
                };
            }
            catch (Exception ex)
            {
                logger.LogError($"Error filtering by Float on {response.Ticker}: {ex.Message}");
                return false;
            }
        }

        private static bool FilterByValue(Filter filter, StocksResponse response)
        {
            return filter.Operator switch
            {
                FilterOperator.gt => response.TickerDetails.Float >= filter.Value,
                FilterOperator.lt => response.TickerDetails.Float <= filter.Value,
                _ => true
            };
        }
    }
}
