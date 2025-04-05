using System.Threading.Tasks;
using MarketDataProvider.Contracts.Models;
using MarketViewer.Contracts.Presentation.Requests;
using MarketViewer.Contracts.Presentation.Responses;

namespace MarketViewer.Contracts.Interfaces
{
    public interface IMarketDataRepository
    {
        public Task<StocksResponse> GetStockDataAsync(StocksRequest request);

    }
}
