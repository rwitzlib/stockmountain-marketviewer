using MarketViewer.Contracts.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Polygon.Client.Interfaces;
using Polygon.Client.Requests;
using Polygon.Client.Responses;
using MarketViewer.Contracts.Requests.Market;
using MarketViewer.Contracts.Responses.Market;
using MarketViewer.Contracts.Enums;

namespace MarketViewer.Infrastructure.Services
{
    public class MarketDataRepository(
        IPolygonClient polygonClient,
        IHttpClientFactory httpClientFactory,
        IMapper mapper,
        ILogger<MarketDataRepository> logger) : IMarketDataRepository
    {
        public async Task<StocksResponse> GetStockDataAsync(StocksRequest request)
        {
            try
            {
                var aggregateRequest = mapper.Map<StocksRequest, PolygonAggregateRequest>(request);
                aggregateRequest.Limit = 50000;

                var polygonResponse = await polygonClient.GetAggregates(aggregateRequest);

                var stocksResponse = mapper.Map<PolygonAggregateResponse, StocksResponse>(polygonResponse);

                return stocksResponse;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error retrieving stock data: {ex.Message}");
                return null;
            }
        }
    }
}
