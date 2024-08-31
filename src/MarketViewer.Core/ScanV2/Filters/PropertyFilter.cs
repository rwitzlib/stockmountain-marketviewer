using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Models.ScanV2.Operands;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Core.ScanV2.Filters;

public class PropertyFilter : IFilterV2
{
    public float[] Compute(IScanOperand operand, StocksResponse stocksResponse, Timeframe timeframe)
    {
        var propertyOperand = operand as PropertyOperand;

        float[] results = propertyOperand.Property switch
        {
            "Float" => [(float)stocksResponse.TickerDetails.Float],
            "MarketCap" => [(float)stocksResponse.TickerDetails.MarketCap],
            _ => []
        };

        return results;
    }
}
