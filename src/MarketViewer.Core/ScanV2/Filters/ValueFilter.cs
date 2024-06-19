using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Core.ScanV2.Filters;

public class ValueFilter : IFilterV2
{
    public float[] Compute(IScanOperand operand, StocksResponse stocksResponse, Timeframe timeframe)
    {
        var valueOperand = operand as ValueOperand;

        var results = new float[timeframe.Multiplier];

        for (int i = 0; i < results.Length; i++)
        {
            results[i] = valueOperand.Value;
        }

        return results;
    }
}
