using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Core.Scanner.Filters
{
    public interface IFilterV2
    {
        bool ApplyFilter(Filter filter, StocksResponse response);
    }
}
