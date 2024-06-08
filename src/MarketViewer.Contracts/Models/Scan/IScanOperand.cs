using MarketViewer.Contracts.Responses;
using Timespan = MarketViewer.Contracts.Enums.Timespan;

namespace MarketViewer.Contracts.Models.Scan;

public interface IScanOperand
{
    public bool HasTimespan(out Timespan? timespan);
    public float[] Compute(StocksResponse stocksResponse, Timeframe timeframe);
}
