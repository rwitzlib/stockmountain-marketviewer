using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Core.ScanV2.Filters
{
    public interface IFilter
    {
        bool ApplyFilter(Filter filter, StocksResponse response);
    }
}
