using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Models.ScanV2.Operands;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Core.ScanV2.Filters;

public class ValueFilter : IFilterV2
{
    public float[] Compute(IScanOperand operand, StocksResponse stocksResponse, Timeframe timeframe)
    {
        var valueOperand = operand as FixedOperand;

        if (timeframe is null)
        {
            return [valueOperand.Value];
        }

        var results = new float[timeframe.Multiplier];
        for (int i = 0; i < timeframe.Multiplier; i++)
        {
            results[i] = valueOperand.Value;
        }

        return results;
    }
}
