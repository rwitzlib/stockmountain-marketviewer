using MarketViewer.Contracts.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketViewer.Contracts.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using MarketViewer.Contracts.Enums;
using MarketViewer.Core.ScanV2;
using MarketViewer.Contracts.Requests.Scan;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Caching;
using Amazon.Runtime.Internal;

namespace MarketViewer.Application.Handlers.Scan;

public class ScanHandlerV2(
    ScanFilterFactoryV2 scanFilterFactory,
    IMarketCache marketCache,
    ILogger<ScanHandlerV2> logger) : IRequestHandler<ScanV2Request, OperationResult<ScanResponse>>
{
    private const int MINIMUM_REQUIRED_CANDLES = 30;
    private const int CANDLES_TO_TAKE = 120;

    private readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");
    private TimeSpan Offset;

    public async Task<OperationResult<ScanResponse>> Handle(ScanV2Request request, CancellationToken cancellationToken)
    {
        Offset = TimeZone.IsDaylightSavingTime(request.Timestamp) ? TimeSpan.FromHours(-5) : TimeSpan.FromHours(-6);

        try
        {
            var sp = new Stopwatch();
            sp.Start();

            var timespans = GetTimespans(request.Argument);
            await InitializeCacheIfEmpty(request.Timestamp.Date, timespans);

            var items = ApplyScanToArgument(request.Argument, request.Timestamp);

            sp.Stop();

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
            if (filter.FirstOperand.HasTimeframe(out var firstMultiplier, out var firstTimespan))
            {
                timespans.Add(firstTimespan.Value);
            }
            if (filter.SecondOperand.HasTimeframe(out var secondMultiplier, out var secondTimespan))
            {
                timespans.Add(secondTimespan.Value);
            }
        }

        var timeSpansFromArgument = GetTimespans(scanArgument.Argument);

        timespans.AddRange(timeSpansFromArgument);

        return timespans.Distinct();
    }

    private async Task InitializeCacheIfEmpty(DateTime date, IEnumerable<Timespan> timespans)
    {
        var tasks = new List<Task>();
        foreach (var timespan in timespans)
        {
            if (marketCache.GetStocksResponse("SPY", timespan, date) is null)
            {
                tasks.Add(Task.Run(async () => await marketCache.Initialize(date, 1, timespan)));
            }
        }
        await Task.WhenAll(tasks);
    }

    private List<ScanResponse.Item> ApplyScanToArgument(ScanArgument argument, DateTimeOffset timestamp)
    {
        if (argument is null || argument.Operator is not "AND" && argument.Operator is not "OR" && argument.Operator is not "AVERAGE" || argument.Filters.Count == 0)
        {
            return [];
        }

        var sortedFitlers = argument.Filters.OrderByDescending(filter => filter.FirstOperand.GetPriority()).ToList();

        List<ScanResponse.Item> results = [];
        for (int i = 0; i < sortedFitlers.Count; i++)
        {
            var filter = sortedFitlers[i];

            if (results.Count == 0 && i > 0)
            {
                return [];
            }

            bool hasTimeframe = filter.FirstOperand.HasTimeframe(out var multiplier, out var timespan);
            var tickersToScan = hasTimeframe ? marketCache.GetTickersByTimespan(timespan.Value, timestamp) : marketCache.GetTickersByTimespan(Timespan.minute, timestamp);
            if (results.Count != 0)
            {
                var currentTickerResults = results.Select(item => item.Ticker);
                tickersToScan = tickersToScan.Where(ticker => currentTickerResults.Contains(ticker));

                // List should reset each time so we can narrow down each time to smaller and smaller list
                results = [];
            }

            foreach (var ticker in tickersToScan)
            {
                var qwer = tickersToScan.Contains("SMCI");

                var stocksResponse = hasTimeframe ? marketCache.GetStocksResponse(ticker, timespan.Value, timestamp) : marketCache.GetStocksResponse(ticker, Timespan.minute, timestamp);

                if (ticker == "SMCI")
                {
                    var asdf = stocksResponse.Results.Where(q => DateTimeOffset.FromUnixTimeMilliseconds(q.Timestamp).ToOffset(Offset).Hour == 12);
                }

                var item = ApplyFilterToStocksResponse(sortedFitlers[i], timestamp, stocksResponse);

                if (item is not null && !results.Any(result => result.Ticker == item.Ticker))
                {
                    results.Add(item);
                }
            }
        }
        return results;
    }

    private ScanResponse.Item ApplyFilterToStocksResponse(FilterV2 filter, DateTimeOffset timestamp, StocksResponse stocksResponse, int candlesToTake = CANDLES_TO_TAKE)
    {
        bool passesFilter = false;

        var reducedStocksResponse = new StocksResponse
        {
            Ticker = stocksResponse.Ticker,
            TickerDetails = stocksResponse.TickerDetails,
            Results = stocksResponse.Results.Where(candle => candle.Timestamp <= timestamp.ToUnixTimeMilliseconds()).TakeLast(candlesToTake).ToList()
        };

        if (reducedStocksResponse.Results is null || reducedStocksResponse.Results.Count < MINIMUM_REQUIRED_CANDLES)
        {
            return null;
        }

        var firstFilter = scanFilterFactory.GetScanFilter(filter.FirstOperand);
        var firstOperandResult = firstFilter.Compute(filter.FirstOperand, reducedStocksResponse, filter.Timeframe);

        var secondFilter = scanFilterFactory.GetScanFilter(filter.SecondOperand);
        var secondOperandResult = secondFilter.Compute(filter.SecondOperand, reducedStocksResponse, filter.Timeframe);

        if (stocksResponse.Ticker == "SMCI")
        {

        }

        if (firstOperandResult.Length == 0 || secondOperandResult.Length == 0)
        {
            return null;
        }

        if (filter.Timeframe is not null)
        {
            if (reducedStocksResponse.Results.Count < filter.Timeframe.Multiplier)
            {
                return null;
            }
        }

        if (filter.CollectionModifier is null)
        {
            passesFilter = filter.Operator switch
            {
                FilterOperator.lt => firstOperandResult.First() < secondOperandResult.First(),
                FilterOperator.le => firstOperandResult.First() <= secondOperandResult.First(),
                FilterOperator.eq => firstOperandResult.First() == secondOperandResult.First(),
                FilterOperator.ge => firstOperandResult.First() >= secondOperandResult.First(),
                FilterOperator.gt => firstOperandResult.First() > secondOperandResult.First(),
                _ => throw new NotImplementedException(),
            };
        }
        else
        {
            passesFilter = filter.CollectionModifier.ToLowerInvariant() switch
            {
                "all" => filter.Operator switch
                {
                    FilterOperator.lt => firstOperandResult.Zip(secondOperandResult, (x, y) => x < y).All(result => result == true),
                    FilterOperator.le => firstOperandResult.Zip(secondOperandResult, (x, y) => x <= y).All(result => result == true),
                    FilterOperator.eq => firstOperandResult.Zip(secondOperandResult, (x, y) => x == y).All(result => result == true),
                    FilterOperator.ge => firstOperandResult.Zip(secondOperandResult, (x, y) => x >= y).All(result => result == true),
                    FilterOperator.gt => firstOperandResult.Zip(secondOperandResult, (x, y) => x > y).All(result => result == true),
                    _ => throw new NotImplementedException(),
                },
                "any" => filter.Operator switch
                {
                    FilterOperator.lt => firstOperandResult.Zip(secondOperandResult, (x, y) => x < y).Any(result => result == true),
                    FilterOperator.le => firstOperandResult.Zip(secondOperandResult, (x, y) => x <= y).Any(result => result == true),
                    FilterOperator.eq => firstOperandResult.Zip(secondOperandResult, (x, y) => x == y).Any(result => result == true),
                    FilterOperator.ge => firstOperandResult.Zip(secondOperandResult, (x, y) => x >= y).Any(result => result == true),
                    FilterOperator.gt => firstOperandResult.Zip(secondOperandResult, (x, y) => x > y).Any(result => result == true),
                    _ => throw new NotImplementedException(),
                },
                "average" => filter.Operator switch
                {
                    FilterOperator.lt => firstOperandResult.Average() < secondOperandResult.Average(),
                    FilterOperator.le => firstOperandResult.Average() <= secondOperandResult.Average(),
                    FilterOperator.eq => firstOperandResult.Average() == secondOperandResult.Average(),
                    FilterOperator.ge => firstOperandResult.Average() >= secondOperandResult.Average(),
                    FilterOperator.gt => firstOperandResult.Average() > secondOperandResult.Average(),
                    _ => throw new NotImplementedException(),
                },
                _ => throw new NotImplementedException()
            };
        }

        if (!passesFilter)
        {
            return null;
        }

        return new ScanResponse.Item
        {
            Ticker = stocksResponse.Ticker,
            Price = stocksResponse.Results.Last().Close,
            Float = stocksResponse.TickerDetails?.Float
        };
    }
    #endregion
}
