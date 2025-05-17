using System.Threading.Tasks;
using MarketDataProvider.Contracts.Models;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Contracts.Interfaces
{
    public interface IMarketDataRepository
    {
        public Task<StocksResponse> GetStockDataAsync(StocksRequest request);

    }
}
