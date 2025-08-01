﻿using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.Scan.Operands;
using MarketViewer.Contracts.Responses.Market;
using MarketViewer.Core.Scan.Filters;

namespace MarketViewer.Core.Scan;

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
