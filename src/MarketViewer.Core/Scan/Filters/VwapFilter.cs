using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses;
using Microsoft.Extensions.Logging;

namespace MarketViewer.Core.Scanner.Filters
{
    public class VwapFilter(ILogger<VwapFilter> logger) : IFilter
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
                            FilterTypeModifier.Slope => FilterBySlope(filter, response),
                            _ => false
                        };

                    default:
                        break;
                };
                return false;
            }
            catch(Exception ex)
            {
                logger.LogError($"Error filtering by VWAP on {response.Ticker}: {ex.Message}");
                return false;
            }
        }
        
        protected static bool FilterByValue(Filter filter, StocksResponse response)
        {
            return true;
        }

        protected static bool FilterBySlope(Filter filter, StocksResponse response)
        {
            return true;
        }
    }
}
