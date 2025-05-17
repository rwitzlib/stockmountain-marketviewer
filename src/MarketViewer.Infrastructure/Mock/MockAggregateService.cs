using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using MarketViewer.Contracts.Interfaces;
using MarketViewer.Contracts.Models;
using System.Diagnostics.CodeAnalysis;
using MarketDataProvider.Contracts.Models;
using Polygon.Client.Models;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;

namespace MarketViewer.Infrastructure.Mock
{
    [ExcludeFromCodeCoverage]
    public class MockAggregateService : IMarketDataRepository
    {
        #region Private Fields
        #endregion

        #region Constructor
        public MockAggregateService() { }
        #endregion

        #region Public Methods
        public Task<StocksResponse> GetStockDataAsync(StocksRequest request)
        {
            //try
            //{
            //    var path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Mock";
            //    var mockMinute = $"{path}/mockMinute.json";

            //    var data = await File.ReadAllTextAsync(mockMinute);

            //    var aggregateResponse = JsonSerializer.Deserialize<PolygonAggregateResponse>(data);

            //    return aggregateResponse;
            //}
            //catch (Exception)
            //{
            //    return null;
            //}
            throw new NotImplementedException();
        }

        public Task<TickerDetails> GetTickerDetailsAsync(string ticker)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
