using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class ValueOperand : IScanOperand
{
    public float Value { get; set; }

    public float[] Compute(StocksResponse stocksResponse, Timeframe timeframe)
    {
        throw new System.NotImplementedException();
    }

    public bool HasTimespan(out Timespan? timespan)
    {
        timespan = null;
        return false;
    }
}
