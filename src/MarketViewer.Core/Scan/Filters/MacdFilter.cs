using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses;
using MarketViewer.Studies;
using Microsoft.Extensions.Logging;
using Polly;

namespace MarketViewer.Core.Scanner.Filters
{
    public class MacdFilter(ILogger<MacdFilter> logger) : IFilter
    {
        private readonly Policy<bool> _retryPolicy = Policy<bool>
               .Handle<ArgumentOutOfRangeException>()
               .RetryForever();

        public bool ApplyFilter(Filter filter, StocksResponse response)
        {
            try
            {
                if (filter is null || response.Results.Count() < 26)
                {
                    return false;
                }

                switch (filter.ValueType)
                {
                    case FilterValueType.CustomAmount:
                        return filter.Modifier switch
                        {
                            FilterTypeModifier.Value => _retryPolicy.Execute(() => FilterByValue(filter, response)),
                            FilterTypeModifier.Slope => _retryPolicy.Execute(() => FilterBySlope(filter, response)),
                            _ => false
                        };

                    default:
                        break;
                };
                return false;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error applying MACD Filter for {response.Ticker}: {ex.Message}");
                return false;
            }
        }

        protected static bool FilterByValue(Filter filter, StocksResponse response)
        {
            var candleData = response.Results.ToArray();

            var macd = MovingAverageConvergenceDivergence.Compute(candleData, 12, 26, 9, "EMA");

            if (macd is null || macd.Lines is null || macd.Lines.Count == 0)
            {
                return false;
            }

            var macdData = macd.Lines[0];

            var macdCandles = macdData.TakeLast(filter.Multiplier);

            return filter.Operator switch
            {
                FilterOperator.gt => macdCandles.Any(q => q.Value > filter.Value),
                FilterOperator.ge => macdCandles.Any(q => q.Value >= filter.Value),
                FilterOperator.lt => macdCandles.Any(q => q.Value < filter.Value),
                FilterOperator.le => macdCandles.Any(q => q.Value <= filter.Value),
                _ => false
            };
        }
        
        protected static bool FilterBySlope(Filter filter, StocksResponse response)
        {
            var candleData = response.Results.ToArray();

            var macd = MovingAverageConvergenceDivergence.Compute(candleData, 12, 26, 9, "EMA");

            if (macd is null || macd.Lines is null || macd.Lines.Count == 0)
            {
                return false;
            }

            var macdData = macd.Lines[0];
            var lastMacdCandles = macdData.TakeLast(filter.Multiplier).ToArray();

            for (var i = 1; i < lastMacdCandles.Length; i++)
            {
                var slope = lastMacdCandles[i].Value - lastMacdCandles[i - 1].Value;

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
    }
}
