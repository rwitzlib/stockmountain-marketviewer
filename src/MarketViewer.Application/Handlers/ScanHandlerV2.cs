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

namespace MarketViewer.Application.Handlers;

public class ScanHandlerV2(
    LiveCache liveCache,
    HistoryCache backtestingCache,
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

        var itemsFromFilterTasks = await Task.WhenAll(applyFilterTasks);
        var itemsFromFilters = new List<ScanResponse.Item>();
        itemsFromFilterTasks.ToList().ForEach(list => itemsFromFilters.AddRange(list));
        var distinctItems = itemsFromFilters.DistinctBy(item => item.Ticker);

        var itemsFromArgument = await argumentTask;

        if (argument.Operator is "AND")
        {
            var results = distinctItems.IntersectBy(itemsFromArgument.Select(item => item.Ticker), item => item.Ticker);

            return results;
        }
        else if (argument.Operator is "OR")
        {
            var results = distinctItems.UnionBy(itemsFromArgument, item => item.Ticker);

            return results;
        }

        return [];
    }

    private static List<ScanResponse.Item> ApplyFilter(FilterV2 filter, StocksResponseCollection stocksResponseCollection)
    {
        var stocksResponses = filter.FirstOperand.HasTimespan(out var timespan) ? stocksResponseCollection.Responses[timespan.Value] : [];

        foreach (var stocksResponse in stocksResponses)
        {
            bool passesFilter = false;

            var firstOperandResult = filter.FirstOperand.Compute(stocksResponse, filter.Timeframe);

            var secondOperandResult = filter.SecondOperand.Compute(stocksResponse, filter.Timeframe);

            switch (filter.CollectionModifier.ToLowerInvariant())
            {
                case "all":
                    switch (filter.Operator)
                    {
                        case FilterOperator.lt:
                            passesFilter = firstOperandResult.All(value => value < secondOperandResult.First());
                            break;

                        case FilterOperator.le:
                            passesFilter = firstOperandResult.All(value => value <= secondOperandResult.First());
                            break;

                        case FilterOperator.eq:
                            passesFilter = firstOperandResult.All(value => value == secondOperandResult.First());
                            break;

                        case FilterOperator.ge:
                            passesFilter = firstOperandResult.All(value => value >= secondOperandResult.First());
                            break;

                        case FilterOperator.gt:
                            passesFilter = firstOperandResult.All(value => value > secondOperandResult.First());
                            break;
                    }
                    break;

                case "any":
                    switch (filter.Operator)
                    {
                        case FilterOperator.lt:
                            passesFilter = firstOperandResult.Any(value => value < secondOperandResult.First());
                            break;

                        case FilterOperator.le:
                            passesFilter = firstOperandResult.Any(value => value <= secondOperandResult.First());
                            break;

                        case FilterOperator.eq:
                            passesFilter = firstOperandResult.Any(value => value == secondOperandResult.First());
                            break;

                        case FilterOperator.ge:
                            passesFilter = firstOperandResult.Any(value => value >= secondOperandResult.First());
                            break;

                        case FilterOperator.gt:
                            passesFilter = firstOperandResult.Any(value => value > secondOperandResult.First());
                            break;
                    }
                    break;

                default:
                    switch (filter.Operator)
                    {
                        case FilterOperator.lt:
                            passesFilter = firstOperandResult.First() < secondOperandResult.First();
                            break;

                        case FilterOperator.le:
                            passesFilter = firstOperandResult.First() <= secondOperandResult.First();
                            break;

                        case FilterOperator.eq:
                            passesFilter = firstOperandResult.First() == secondOperandResult.First();
                            break;

                        case FilterOperator.ge:
                            passesFilter = firstOperandResult.First() >= secondOperandResult.First();
                            break;

                        case FilterOperator.gt:
                            passesFilter = firstOperandResult.First() > secondOperandResult.First();
                            break;
                    }
                    break;
            }
        }
        
        return null;
    }
    #endregion
}
