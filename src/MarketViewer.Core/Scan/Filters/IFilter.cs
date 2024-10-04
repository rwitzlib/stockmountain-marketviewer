using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Core.Scanner.Filters
{
    public interface IFilter
    {
        bool ApplyFilter(Filter filter, StocksResponse response);
    }
}
