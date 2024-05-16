using System;
using System.Threading.Tasks;
using MarketViewer.Contracts.Interfaces;
using MarketViewer.Contracts.Responses;
using AutoMapper;
using MarketViewer.Contracts.Requests;
using MarketDataProvider.Contracts.Requests;
using System.Linq;
using MarketDataProvider.Contracts.Dtos;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using MarketDataProvider.Contracts.Responses;
using Microsoft.Extensions.Logging;
using Polygon.Client.Interfaces;
using MarketDataProvider.Clients.Interfaces;
using Polygon.Client.Models;
using Polygon.Client.Requests;
using Polygon.Client.Responses;

namespace MarketViewer.Infrastructure.Services
{
    public class MarketDataRepository(
        IMarketCacheClient marketCacheClient,
        IPolygonClient polygonClient,
        IHttpClientFactory httpClientFactory,
        IMapper mapper,
        ILogger<MarketDataRepository> logger) : IMarketDataRepository
    {
        public async Task<StocksResponse> GetStockDataAsync(StocksRequest request)
        {
            //StocksResponse stocksResponse = null;
            //StocksResponse stocksResponseFromPolygon = null;
            try
            {
                //var tasks = new List<Task>();
                var aggregateRequest = mapper.Map<StocksRequest, AggregateRequest>(request);
                aggregateRequest.To = aggregateRequest.To.AddDays(1).AddMinutes(-1);

                var response = await GetStockDataFromPolygon(aggregateRequest);

                return response;
                //var days = DateUtilities.GetMarketOpenDays(aggregateRequest.From, aggregateRequest.To);

                ////if (await TryGetStockDataFromMarketCache(aggregateRequest, days, out var cacheResponse))
                ////{

                ////}

                //if (aggregateRequest.To.Date == DateTimeOffset.Now.Date)
                //{
                //    aggregateRequest.From = aggregateRequest.To.Date;
                //    stocksResponseFromPolygon = await GetStockDataFromPolygon(aggregateRequest);

                //    var adjustedDate = aggregateRequest.To.AddDays(-1);
                //    aggregateRequest.To = adjustedDate;
                //    aggregateRequest.From = request.From;
                //}


                //if (days.Count > 0)
                //{
                //    stocksResponse = await GetStocksResponseFromCache(aggregateRequest, days) 
                //        ?? await GetStocksResponseFromMarketDataProvider(aggregateRequest, days);
                //}

                //if (stocksResponse is null)
                //{
                //    stocksResponse = stocksResponseFromPolygon;
                //}
                //else
                //{
                //    if (stocksResponseFromPolygon is not null)
                //    {
                //        stocksResponse.Results.AddRange(stocksResponseFromPolygon.Results);
                //    }
                //}

                //var index = stocksResponse.Results.FindIndex(candle => candle.Timestamp > request.To.ToUnixTimeMilliseconds());

                //if (index >= 0 )
                //{
                //    stocksResponse.Results.RemoveRange(index, stocksResponse.Results.Count() - index);
                //}

                //return stocksResponse;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error retrieving stock data: {ex.Message}");
                return null;
            }
        }

        //private async Task<bool> TryGetStockDataFromMarketCache(AggregateRequest request, List<DateTime> days, out StocksResponse response)
        //{
        //    response = null;
        //    try
        //    {
        //        var cacheResponse = await marketCacheClient.QueryAggregates<AggregateDto>(request);

        //        if (cacheResponse is not null && cacheResponse.ToList().Count == days.Count)
        //        {
        //            response = mapper.Map<IEnumerable<AggregateDto>, StocksResponse>(cacheResponse);
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError($"Error getting stock data for {request.Ticker}: {ex.Message}");
        //    }
        //    return false;
        //}

        private async Task<StocksResponse> GetStockDataFromPolygon(AggregateRequest request)
        {
            try
            {
                var polygonRequest = mapper.Map<AggregateRequest, PolygonAggregateRequest>(request);
                //polygonRequest.To = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

                var polygonResponse = await polygonClient.GetAggregates(polygonRequest);

                var stocksResponse = mapper.Map<PolygonAggregateResponse, StocksResponse>(polygonResponse);

                return stocksResponse;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error aggregate data for {request.Ticker}: {ex.Message}");
                return null;
            }
        }

        private async Task<StocksResponse> GetStocksResponseFromMarketDataProvider(AggregateRequest request, List<DateTime> days)
        {
            try
            {
                // TODO: If days are missing, target specific days to speed this up?
                var aggregateUrl = "/api/aggregate";
                var client = httpClientFactory.CreateClient("marketdataprovider");

                var response = await client.PostAsJsonAsync(aggregateUrl, request);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError($"Invalid stock data response for {request.Ticker}.");
                    return null;
                }

                var aggregateResponse = await response.Content.ReadFromJsonAsync<AggregateResponse>();

                var stocksResponse = mapper.Map<AggregateResponse, StocksResponse>(aggregateResponse);

                return stocksResponse;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error getting stock data for {request.Ticker}: {ex.Message}");
                return null;
            }
        }

        private async Task<TickerDetails> GetTickerDetailsFromCacheOrMarketDataProvider(string ticker)
        {
            try
            {
                // TODO: Try looking in cache first
                var tickerDetailsUrl = $"/api/tickerdetails/{ticker}";
                var client = httpClientFactory.CreateClient("marketdataprovider");

                var tickerDetailsResponse = await client.GetAsync(tickerDetailsUrl);

                if (!tickerDetailsResponse.IsSuccessStatusCode)
                {
                    logger.LogError($"Invalid ticker response for for {ticker}.");
                    return null;
                }

                var tickerDetails = await tickerDetailsResponse.Content.ReadFromJsonAsync<TickerDetails>();

                return tickerDetails;
            }
            catch (Exception ex)
            {
                logger.LogError($"Error getting ticker details for {ticker}: {ex.Message}");
                return null;
            }
        }
    }
}
