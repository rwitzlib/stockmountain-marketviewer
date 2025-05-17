using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Scan.Operands;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Core.Scan.Filters;

public class PriceActionFilter : IFilterV2
{
    public float[] Compute(IScanOperand operand, StocksResponse stocksResponse, Timeframe timeframe)
    {
        var priceActionOperand = operand as PriceActionOperand;

        var candles = priceActionOperand.PriceAction switch
        {
            PriceActionType.Open => stocksResponse.Results.Select(q => q.Open),
            PriceActionType.Close => stocksResponse.Results.Select(q => q.Close),
            PriceActionType.High => stocksResponse.Results.Select(q => q.High),
            PriceActionType.Low => stocksResponse.Results.Select(q => q.Low),
            PriceActionType.Vwap => stocksResponse.Results.Select(q => q.Vwap),
            PriceActionType.Volume => stocksResponse.Results.Select(q => q.Volume),
            _ => [],
        };

        var values = priceActionOperand.Modifier switch
        {
            OperandModifier.Value => candles,
            OperandModifier.Slope => GetSlope(candles.ToArray()),
            _ => []
        };

        return values.TakeLast(timeframe.Multiplier).ToArray();
    }

    private static float[] GetSlope(float[] values)
    {
        if (values.Length < 2)
        {
            return [];
        }

        var results = new float[values.Length - 1];

        for (int i = 0; i < values.Length - 1; i++)
        {
            results[i] = values[i + 1] - values[i];
        }

        return results;
    }
}
