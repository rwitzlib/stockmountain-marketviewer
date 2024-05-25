using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses;
using Microsoft.Extensions.Logging;

namespace MarketViewer.Core.ScanV2.Filters
{
    public class VolumeFilter : IFilter
    {
        private readonly ILogger<VolumeFilter> _logger;

        public VolumeFilter(ILogger<VolumeFilter> logger)
        {
            _logger = logger;
        }

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
                _logger.LogError($"Error filtering by Volume on {response.Ticker}: {ex.Message}");
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
