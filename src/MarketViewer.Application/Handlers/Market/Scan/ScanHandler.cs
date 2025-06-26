using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Caching;
using MarketViewer.Core.Scan;
using MarketViewer.Contracts.Mappers;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Requests.Market.Scan;
using MarketViewer.Contracts.Responses.Market;
using MarketViewer.Infrastructure.Utilities;
using Polygon.Client.Models;
using Amazon.Runtime.Internal;

namespace MarketViewer.Application.Handlers.Market.Scan;

public class ScanHandler(
    ScanFilterFactoryV2 scanFilterFactory,
    IMarketCache marketCache,
    ILogger<ScanHandler> logger) : IRequestHandler<ScanRequest, OperationResult<ScanResponse>>
{
    private const int MINIMUM_REQUIRED_CANDLES = 30;
    private const int CANDLES_TO_TAKE = 120;

    //private readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Chicago");

    public async Task<OperationResult<ScanResponse>> Handle(ScanRequest request, CancellationToken cancellationToken)
    {
        //TimeSpan offset = TimeZone.IsDaylightSavingTime(request.Timestamp) ? TimeSpan.FromHours(-5) : TimeSpan.FromHours(-6);

        try
        {
            var sp = new Stopwatch();
            sp.Start();

            var scanArgument = ScanArgumentMapper.ConvertFromScanArgumentDto(request.Argument);

            var timespans = ScanUtilities.GetTimespans(scanArgument);
            await InitializeCacheIfEmpty(request.Timestamp.Date, timespans);

            var items = ApplyScanToArgument(scanArgument, request.Timestamp);

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
            logger.LogError(ex, "Error scanning for {timestamp}: {message}", request.Timestamp, ex.Message);
            return new OperationResult<ScanResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal Error."]
            };
        }
    }

    #region Private Methods
    private async Task InitializeCacheIfEmpty(DateTime date, IEnumerable<Timespan> timespans)
    {
        var tasks = new List<Task>();
        foreach (var timespan in timespans)
        {
            if (marketCache.GetStocksResponse("SPY", new Timeframe(1, timespan), date) is null)
            {
                tasks.Add(Task.Run(async () => await marketCache.Initialize(date, new Timeframe(1, timespan))));
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

            bool hasTimeframe = filter.FirstOperand.HasTimeframe(out var timeframe);
            var tickersToScan = hasTimeframe ? marketCache.GetTickersByTimeframe(timeframe, timestamp) : marketCache.GetTickersByTimeframe(new Timeframe(1, Timespan.minute), timestamp);
            if (results.Count != 0)
            {
                var currentTickerResults = results.Select(item => item.Ticker);
                tickersToScan = tickersToScan.Where(ticker => currentTickerResults.Contains(ticker));

                // List should reset each time so we can narrow down each time to smaller and smaller list
                results = [];
            }

            foreach (var ticker in tickersToScan)
            {
                //if (!IsMinuteResponseValid(ticker, timestamp))
                //{
                //    continue;
                //}

                var stocksResponse = hasTimeframe ? marketCache.GetStocksResponse(ticker, timeframe, timestamp) : marketCache.GetStocksResponse(ticker, new Timeframe(1, Timespan.minute), timestamp);

                if (stocksResponse is null)
                {
                    logger.LogWarning("No stocks response found for ticker {ticker} at {timestamp}", ticker, timestamp);
                    continue;
                }

                var clonedResponse = stocksResponse.Clone();

                var latestBar = marketCache.GetLiveBar(ticker);
                
                if (hasTimeframe)
                {
                    TryAddBarToResponse(timeframe.Multiplier, timeframe.Timespan, latestBar, clonedResponse);
                }
                else
                {
                    TryAddBarToResponse(1, Timespan.minute, latestBar, clonedResponse);
                }

                var item = ApplyFilterToStocksResponse(sortedFitlers[i], timestamp, clonedResponse);

                if (item is not null && !results.Any(result => result.Ticker == item.Ticker))
                {
                    results.Add(item);
                }
            }
        }
        return results;
    }

    private static void TryAddBarToResponse(int multiplier, Timespan timespan, Bar latestBar, StocksResponse response)
    {
        if (latestBar is null || response is null || response.Results?.Count <= 0 || latestBar.Timestamp <= response.Results.Last().Timestamp)
        {
            return;
        }

        switch (timespan)
        {
            case Timespan.minute:
                if (multiplier != 1)
                {
                    return; // Only add live bar for 1 minute aggregates
                }
                response.Results.Add(latestBar);
                break;
            case Timespan.hour:
                if (multiplier != 1)
                {
                    return; // Only add live bar for 1 hour aggregates
                }
                var last = response.Results.Last();

                if (last.Timestamp + (60 * 60000) < latestBar.Timestamp)
                {
                    response.Results.Add(latestBar);
                }
                else
                {
                    // Update the last bar with the latest data
                    last.Close = latestBar.Close;
                    last.High = Math.Max(last.High, latestBar.High);
                    last.Low = Math.Min(last.Low, latestBar.Low);
                    last.Volume += latestBar.Volume;
                    last.Vwap = (last.Close + last.High + last.Low) / 3;
                }
                break;
            case Timespan.day:
            case Timespan.week:
            case Timespan.month:
            case Timespan.quarter:
            case Timespan.year:
                return;
            default:
                throw new NotImplementedException();
        }
    }

    private ScanResponse.Item ApplyFilterToStocksResponse(Filter filter, DateTimeOffset timestamp, StocksResponse stocksResponse, int candlesToTake = CANDLES_TO_TAKE)
    {
        try
        {
            bool passesFilter = false;

            if (stocksResponse is null || stocksResponse.Results is null || stocksResponse.Results.Count == 0)
            {
                return null;
            }

            var reducedStocksResponse = new StocksResponse
            {
                Ticker = stocksResponse.Ticker,
                TickerInfo = stocksResponse.TickerInfo,
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
                Float = stocksResponse.TickerInfo?.TickerDetails?.Float
            };
        }
        catch (Exception e)
        {
            logger.LogError("Error applying filter {filter} to stocks response for {ticker} at {timestamp}: {message}", filter, stocksResponse?.Ticker, timestamp, e.Message);
            return null;
        }
    }
    #endregion
}
