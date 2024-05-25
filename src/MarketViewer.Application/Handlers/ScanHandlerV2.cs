using MarketViewer.Contracts.Responses;
using MarketViewer.Infrastructure.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketViewer.Contracts.Interfaces;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Requests;
using Microsoft.Extensions.Logging;
using MarketViewer.Core.Scanner;
using System.Net;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Enums;
using MarketViewer.Core.ScanV2;

namespace MarketViewer.Application.Handlers;

public class ScanHandlerV2(
    LiveCache liveCache,
    HistoryCache backtestingCache,
    ScanFilterFactory scanFilterFactory,
    ILogger<ScanHandlerV2> logger) : IRequestHandler<ScanRequestV2, OperationResult<ScanResponse>>
{
    public async Task<OperationResult<ScanResponse>> Handle(ScanRequestV2 request, CancellationToken cancellationToken)
    {
        try
        {
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
                tasks.Add(Task.Run(() => ApplyScanToArgument(request.Arguments, stocksResponse)));
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
    private static bool IsDateTimeToday(DateTimeOffset date)
    {
        return date.ToString("yyyy-MM-dd").Equals(DateTime.Now.ToString("yyyy-MM-dd"));
    }

    private ScanResponse.Item ApplyScanToArgument(ScanArgument argument, StocksResponse stocksResponse)
    {
        ScanResponse.Item item = null;

        if (argument.Operator is not "AND" || argument.Operator is not "OR" || argument.Filters.Length == 0)
        {
            return null;
        }

        if (argument.Argument is not null)
        {
            item = ApplyScanToArgument(argument.Argument, stocksResponse);
        }

        List<bool> results = [];
        results.AddRange(from filter in argument.Filters
                         select ApplyFilter(filter, stocksResponse));

        if (argument.Operator is "AND")
        {
            if ((argument.Argument is not null && item is null) || !results.All(result => result == true))
            {
                return null;
            }
        }
        else if (argument.Operator is "OR")
        {
            if ((argument.Argument is not null && item is null) || !results.Any(result => result == true))
            {
                return null;
            }
        }

        //var multiplier = filters.Max(q => q.Multiplier);
        //var lastCandles = stocksResponse.Results.TakeLast(multiplier);

        return new ScanResponse.Item
        {
            Ticker = stocksResponse.Ticker,
            Price = stocksResponse.Results.Last().Close,
            //Volume = lastCandles.Sum(x => x.Volume),
            Float = stocksResponse.TickerDetails?.Float
        };
    }

    private bool ApplyFilter(FilterV2 filter, StocksResponse stocksResponse)
    {


        return true;
    }
    #endregion
}
