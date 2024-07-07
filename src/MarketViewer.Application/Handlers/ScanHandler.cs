using MarketViewer.Contracts.Responses;
using MarketViewer.Infrastructure.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Requests;
using Microsoft.Extensions.Logging;
using MarketViewer.Core.Scanner;
using System.Net;
using MarketViewer.Contracts.Models.Scan;

namespace MarketViewer.Application.Handlers;

public class ScanHandler(
    LiveCache liveCache,
    HistoryCache backtestingCache,
    ScanFilterFactory scanFilterFactory,
    ILogger<ScanHandler> logger) : IRequestHandler<ScanRequest, OperationResult<ScanResponse>>
{
    public async Task<OperationResult<ScanResponse>> Handle(ScanRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ValidateScanRequest(request, out var errorMessages))
            {
                return new OperationResult<ScanResponse>
                {
                    Status = HttpStatusCode.BadRequest,
                    ErrorMessages = errorMessages
                };
            }

            var sp = new Stopwatch();
            sp.Start();

            IEnumerable<StocksResponse> stocksResponses;

            if (IsDateTimeToday(request.Timestamp))
            {
                stocksResponses = liveCache.GetStocksResponses(request.Timestamp);
            }
            else
            {
                stocksResponses = await backtestingCache.GetStocksResponses(request.Timestamp);
            }
            logger.LogInformation("Total StocksResponses found: {count}", stocksResponses.Count());

            var tasks = new List<Task<ScanResponse.Item>>();
            foreach (var stocksResponse in stocksResponses)
            {
                tasks.Add(Task.Run(() => ApplyFilters(request.Filters, stocksResponse)));
            }
            var results = await Task.WhenAll(tasks);

            sp.Stop();

            logger.LogInformation("Total StocksResponses after filtering: {count}", results.Count(q => q is not null));
            return new OperationResult<ScanResponse>
            {
                Status = HttpStatusCode.OK,
                Data = new ScanResponse
                {
                    Items = results.Where(item => item is not null),
                    TimeElapsed = sp.ElapsedMilliseconds
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError($"Error scanning for {request.Timestamp}: {ex.Message}");
            return new OperationResult<ScanResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal Error."]
            };
        }
    }

    #region Private Methods
    private static bool ValidateScanRequest(ScanRequest request, out List<string> errorMessages)
    {
        errorMessages = [];

        if (request.Filters is null || !request.Filters.Any())
        {
            errorMessages.Add("No filters.");
        }

        return errorMessages.Count == 0;
    }

    private static bool IsDateTimeToday(DateTimeOffset date)
    {
        return date.ToString("yyyy-MM-dd").Equals(DateTime.Now.ToString("yyyy-MM-dd"));
    }

    private ScanResponse.Item ApplyFilters(IEnumerable<Filter> filters, StocksResponse stocksResponse)
    {
        var sortedFilters = filters.OrderBy(filter => filter.Type);

        foreach (var filter in sortedFilters)
        {
            var filterService = scanFilterFactory.GetScanFilter(filter);

            if (!filterService.ApplyFilter(filter, stocksResponse))
            {
                return null;
            }
        }

        var multiplier = filters.Max(q => q.Multiplier);
        var lastCandles = stocksResponse.Results.TakeLast(multiplier);

        return new ScanResponse.Item
        {
            Ticker = stocksResponse.Ticker,
            Price = stocksResponse.Results.Last().Close,
            Volume = lastCandles.Sum(x => x.Volume),
            Float = stocksResponse.TickerDetails?.Float
        };
    }
    #endregion
}
