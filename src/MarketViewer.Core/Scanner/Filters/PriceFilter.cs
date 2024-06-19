using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses;
using MarketViewer.Studies;
using Microsoft.Extensions.Logging;

namespace MarketViewer.Core.Scanner.Filters
{
    public class PriceFilter(ILogger<PriceFilter> logger) : IFilter
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
                        FilterTypeModifier.Slope => FilterBySlope(filter, response),
                        _ => false
                    },
                    FilterValueType.Vwap => FilterByVwap(filter, response),
                    _ => false,
                };
            }
            catch(Exception ex)
            {
                logger.LogError($"Error filtering by Price on {response.Ticker}: {ex.Message}");
                return false;
            }
        }
        
        protected static bool FilterByValue(Filter filter, StocksResponse response)
        {
            var candleData = response.Results;

            var candles = candleData.TakeLast(filter.Multiplier);

            return filter.Operator switch
            {
                FilterOperator.gt => candles.Any(q => q.Close > filter.Value),
                FilterOperator.lt => candles.Any(q => q.Close < filter.Value),
                _ => false
            };
        }

        protected static bool FilterBySlope(Filter filter, StocksResponse response)
        {
            var candleData = response.Results;
            var candles = candleData.TakeLast(filter.Multiplier).ToArray();

            for (var i = 1; i < candles.Length; i++)
            {
                var slope = candles[i].Close - candles[i - 1].Close;

                switch (filter.Operator)
                {
                    case FilterOperator.gt:
                        if (slope <= filter.Value)
                        {
                            return false;
                        }
                        break;

                    case FilterOperator.ge:
                        if (slope < filter.Value)
                        {
                            return false;
                        }
                        break;

                    case FilterOperator.lt:
                        if (slope >= filter.Value)
                        {
                            return false;
                        }
                        break;

                    case FilterOperator.le:
                        if (slope > filter.Value)
                        {
                            return false;
                        }
                        break;

                    case FilterOperator.eq:
                        if (!slope.Equals(filter.Value))
                        {
                            return false;
                        }
                        break;

                    default:
                        return false;
                };
            }

            return true;
        }

        protected static bool FilterByVwap(Filter filter, StocksResponse response)
        {
            var candleData = response.Results.ToArray();

            var vwapSeries = VolumeWeightedAveragePrice.Compute(candleData).Lines[0];

            var lastAggregateCandles = candleData.TakeLast(filter.Multiplier).ToArray();
            var lastVwapCandles = vwapSeries.TakeLast(filter.Multiplier).ToArray();

            if (vwapSeries.Count != candleData.Count())
            {
                return false;
            }

            for (var i = 0; i < filter.Multiplier; i++)
            {
                var currentPrice = lastAggregateCandles[i].Close;
                var currentVwap = lastVwapCandles[i].Value;

                switch (filter.Operator)
                {
                    case FilterOperator.gt:
                        if (currentPrice > currentVwap)
                        {
                            return true;
                        }
                        break;

                    case FilterOperator.ge:
                        if (currentPrice >= currentVwap)
                        {
                            return true;
                        }
                        break;

                    case FilterOperator.lt:
                        if (currentPrice < currentVwap)
                        {
                            return true;
                        }
                        break;

                    case FilterOperator.le:
                        if (currentPrice <= currentVwap)
                        {
                            return true;
                        }
                        break;

                    case FilterOperator.eq:
                        if (currentPrice == currentVwap)
                        {
                            return true;
                        }
                        break;

                    default:
                        return false;
                };
            }
            return false;
        }
    }
}
