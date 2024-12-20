using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.ScanV2.Operands;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Core.ScanV2.Filters;

public class PropertyFilter(MemoryMarketCache marketCache) : IFilterV2
{
    public float[] Compute(IScanOperand operand, StocksResponse stocksResponse, Timeframe timeframe)
    {
        try
        {
            var tickerDetails = marketCache.GetTickerDetails(stocksResponse.Ticker);

            if (tickerDetails is null)
            {
                return [];
            }

            var propertyOperand = operand as PropertyOperand;

            float[] results = propertyOperand.Property switch
            {
                "Float" => [tickerDetails.Float],
                "MarketCap" => [(float)tickerDetails.MarketCap],
                _ => []
            };

            return results;
        }
        catch (Exception ex)
        {
            return [];
        }
    }
}
