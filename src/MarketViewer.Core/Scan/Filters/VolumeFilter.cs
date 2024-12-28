using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses;
using MarketViewer.Core.Scanner.Filters;
using Microsoft.Extensions.Logging;

namespace MarketViewer.Core.Scan.Filters
{
    public class VolumeFilter(ILogger<VolumeFilter> logger) : IFilter
    {
        public bool ApplyFilter(Filter filter, StocksResponse response)
        {
            try
            {
                if (filter is null || !response.Results.Any())
                {
                    return false;
                }

                switch (filter.ValueType)
                {
                    case FilterValueType.CustomAmount:
                        return filter.Modifier switch
                        {
                            FilterTypeModifier.Value => FilterByValue(filter, response),
                            _ => false
                        };

                    default:
                        break;
                };
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error filtering by Volume on {response.Ticker}: {ex.Message}");
                return false;
            }
        }

        protected static bool FilterByValue(Filter filter, StocksResponse response)
        {
            var candleData = response.Results;

            var totalVolume = candleData.TakeLast(filter.Multiplier).Sum(q => q.Volume);

            return filter.Operator switch
            {
                FilterOperator.gt => totalVolume > filter.Value,
                FilterOperator.ge => totalVolume >= filter.Value,
                FilterOperator.lt => totalVolume < filter.Value,
                FilterOperator.le => totalVolume <= filter.Value,
                _ => false
            };
        }
    }
}
