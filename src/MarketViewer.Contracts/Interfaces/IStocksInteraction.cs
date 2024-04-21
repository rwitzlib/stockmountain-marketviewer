using System.Threading.Tasks;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Contracts.Interfaces
{
    public interface IStocksInteraction
    {
        public Task<OperationResult<StocksResponse>> GetStockDataAsync(StocksRequest aggregateRequest);
    }
}
