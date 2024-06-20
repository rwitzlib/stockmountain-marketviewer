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
using System.Net;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Enums;
using MarketViewer.Core.ScanV2;
using MarketViewer.Contracts.Comparers;
using MarketViewer.Contracts.Models.ScanV2;

namespace MarketViewer.Application.Handlers;

public class ScanHandlerV2(
    LiveCache liveCache,
    HistoryCache backtestingCache,
    ScanFilterFactoryV2 scanFilterFactory,
    ILogger<ScanHandlerV2> logger) : IRequestHandler<ScanRequestV2, OperationResult<ScanResponse>>
{
    public async Task<OperationResult<ScanResponse>> Handle(ScanRequestV2 request, CancellationToken cancellationToken)
    {
        try
        {
            var sp = new Stopwatch();
            sp.Start();

            StocksResponseCollection stocksResponseCollection;
            var timespans = GetTimespans(request.Argument);

            if (IsDateTimeToday(request.Timestamp))
            {
                stocksResponseCollection = liveCache.GetStocksResponses(request.Timestamp, timespans);
            }
            else
            {
                stocksResponseCollection = await backtestingCache.GetStocksResponses(request.Timestamp, timespans);
            }

            //logger.LogInformation("Total StocksResponses found: {count}", stocksResponseCollection.Count());

            var items = await ApplyScanToArgument(request.Argument, stocksResponseCollection);

            sp.Stop();

            //logger.LogInformation("Total StocksResponses after filtering: {count}", results.Count(result => result is not null));
            return new OperationResult<ScanResponse>
            {
                Status = HttpStatusCode.OK,
                Data = new ScanResponse
                {
                    Items = items,
                    TimeElapsed = sp.ElapsedMilliseconds
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError("Error scanning for {timestamp}: {message}", request.Timestamp, ex.Message);
            return new OperationResult<ScanResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal Error."]
            };
        }
    }

    #region Private Methods
    private static IEnumerable<Timespan> GetTimespans(ScanArgument scanArgument)
    {
        if (scanArgument == null)
        {
            return [];
        }

        var timespans = new List<Timespan>();

        foreach (var filter in scanArgument.Filters)
        {
            if (filter.FirstOperand.HasTimespan(out var firstTimespan))
            {
                timespans.Add(firstTimespan.Value);
            }
            if (filter.SecondOperand.HasTimespan(out var secondTimespan))
            {
                timespans.Add(secondTimespan.Value);
            }
        }

        var timeSpansFromArgument = GetTimespans(scanArgument.Argument);

        timespans.AddRange(timeSpansFromArgument);

        return timespans.Distinct();
    }
    
    private static bool IsDateTimeToday(DateTimeOffset date)
    {
        return date.ToString("yyyy-MM-dd").Equals(DateTime.Now.ToString("yyyy-MM-dd"));
    }

    private async Task<IEnumerable<ScanResponse.Item>> ApplyScanToArgument(ScanArgument argument, StocksResponseCollection stocksResponseCollection)
    {
        if (argument is null || (argument.Operator is not "AND" && argument.Operator is not "OR") || argument.Filters.Length == 0)
        {
            return [];
        }

        var argumentTask = Task.Run(() => ApplyScanToArgument(argument.Argument, stocksResponseCollection));

        List<Task<List<ScanResponse.Item>>> applyFilterTasks = [];
        foreach (var filter in argument.Filters)
        {
            applyFilterTasks.Add(Task.Run(() => ApplyFilter(filter, stocksResponseCollection)));
        }

        var itemsFromFilters = await Task.WhenAll(applyFilterTasks);
        var itemsFromArgument = await argumentTask;

        switch (argument.Operator.ToLowerInvariant())
        {
            case "and":
                {
                    var intersection = itemsFromFilters
                        .Aggregate<IEnumerable<ScanResponse.Item>>((previous, next) => previous.Intersect(next, new ScanResponseItemComparer()));

                    if (argument.Argument is null)
                    {
                        return intersection;
                    }

                    return intersection.Intersect(itemsFromArgument, new ScanResponseItemComparer());
                }
            case "or":
                {
                    var union = itemsFromFilters
                        .Aggregate<IEnumerable<ScanResponse.Item>>((previous, next) => previous.UnionBy(next, item => item.Ticker));

                    if (argument.Argument is null)
                    {
                        return union;
                    }

                    return union.UnionBy(itemsFromArgument, item => item.Ticker);
                }
            default:
                {
                    return [];
                }
        }
    }

    private List<ScanResponse.Item> ApplyFilter(FilterV2 filter, StocksResponseCollection stocksResponseCollection)
    {
        var results = new List<ScanResponse.Item>();
        var stocksResponses = filter.FirstOperand.HasTimespan(out var timespan) ? stocksResponseCollection.Responses[timespan.Value] : [];

        foreach (var stocksResponse in stocksResponses)
        {
            bool passesFilter = false;

            var firstFilter = scanFilterFactory.GetScanFilter(filter.FirstOperand);
            var firstOperandResult = firstFilter.Compute(filter.FirstOperand, stocksResponse, filter.Timeframe);

            var secondFilter = scanFilterFactory.GetScanFilter(filter.SecondOperand);
            var secondOperandResult = secondFilter.Compute(filter.SecondOperand, stocksResponse, filter.Timeframe);

            if (firstOperandResult.Length == 0 || secondOperandResult.Length == 0)
            {
                continue;
            }

            if (firstOperandResult.Length < secondOperandResult.Length)
            {
                secondOperandResult = secondOperandResult.TakeLast(firstOperandResult.Length).ToArray();
            }
            else if (firstOperandResult.Length > secondOperandResult.Length)
            {
                firstOperandResult = firstOperandResult.TakeLast(secondOperandResult.Length).ToArray();
            }

            if (firstOperandResult.Length != secondOperandResult.Length)
            {
                return null;
            }

            switch (filter.CollectionModifier.ToLowerInvariant())
            {
                case "all":
                    switch (filter.Operator)
                    {
                        case FilterOperator.lt:
                            passesFilter = firstOperandResult.Zip(secondOperandResult, (x, y) => x < y).All(result => result == true);
                            break;

                        case FilterOperator.le:
                            passesFilter = firstOperandResult.Zip(secondOperandResult, (x, y) => x <= y).All(result => result == true);
                            break;

                        case FilterOperator.eq:
                            passesFilter = firstOperandResult.Zip(secondOperandResult, (x, y) => x == y).All(result => result == true);
                            break;

                        case FilterOperator.ge:
                            passesFilter = firstOperandResult.Zip(secondOperandResult, (x, y) => x >= y).All(result => result == true);
                            break;

                        case FilterOperator.gt:
                            passesFilter = firstOperandResult.Zip(secondOperandResult, (x, y) => x > y).All(result => result == true);
                            break;
                    }
                    break;

                case "any":
                    switch (filter.Operator)
                    {
                        case FilterOperator.lt:
                            passesFilter = firstOperandResult.Zip(secondOperandResult, (x, y) => x < y).Any(result => result == true);
                            break;

                        case FilterOperator.le:
                            passesFilter = firstOperandResult.Zip(secondOperandResult, (x, y) => x <= y).Any(result => result == true);
                            break;

                        case FilterOperator.eq:
                            passesFilter = firstOperandResult.Zip(secondOperandResult, (x, y) => x == y).Any(result => result == true);
                            break;

                        case FilterOperator.ge:
                            passesFilter = firstOperandResult.Zip(secondOperandResult, (x, y) => x >= y).Any(result => result == true);
                            break;

                        case FilterOperator.gt:
                            passesFilter = firstOperandResult.Zip(secondOperandResult, (x, y) => x > y).Any(result => result == true);
                            break;
                    }
                    break;

                default:
                    switch (filter.Operator)
                    {
                        case FilterOperator.lt:
                            passesFilter = firstOperandResult.Last() < secondOperandResult.Last();
                            break;

                        case FilterOperator.le:
                            passesFilter = firstOperandResult.Last() <= secondOperandResult.Last();
                            break;

                        case FilterOperator.eq:
                            passesFilter = firstOperandResult.Last() == secondOperandResult.Last();
                            break;

                        case FilterOperator.ge:
                            passesFilter = firstOperandResult.Last() >= secondOperandResult.Last();
                            break;

                        case FilterOperator.gt:
                            passesFilter = firstOperandResult.Last() > secondOperandResult.Last();
                            break;
                    }
                    break;
            }

            if (passesFilter)
            {
                results.Add(new ScanResponse.Item
                {
                    Ticker = stocksResponse.Ticker,
                    Price = stocksResponse.Results.Last().Close,
                    Volume = stocksResponse.Results.TakeLast(filter.Timeframe.Multiplier).Select(q => q.Volume).Sum()
                    //Float = stocksResponse.TickerDetails.Float
                });
            }
        }
        
        return results;
    }
    #endregion
}
