using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Models.ScanV2.Operands;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Core.ScanV2.Filters;

public interface IFilterV2
{
    public float[] Compute(IScanOperand operand, StocksResponse stocksResponse, Timeframe timeframe);
}
