using Amazon.Lambda;
using Amazon.Lambda.Model;
using MarketViewer.Contracts.Enums.Backtest;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Records;
using MarketViewer.Contracts.Requests.Market.Backtest;
using MarketViewer.Contracts.Responses.Market.Backtest;
using MarketViewer.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.IO.Compression;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MarketViewer.Infrastructure.Config;
using System.Collections.Generic;
using System.Linq;
using MarketViewer.Infrastructure.Utilities;

namespace MarketViewer.Application.Handlers.Market.Backtest;

public class BacktestHandler(
    BacktestConfig config,
    IAmazonLambda lambda,
    IBacktestRepository repository,
    ILogger<BacktestHandler> logger)
{
    private readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");


    public async Task<OperationResult<BacktestEntryResponse>> Create(BacktestCreateRequest request)
    {
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Id))
            {
                return new OperationResult<BacktestEntryResponse>
                {
                    Status = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Invalid request."]
                };
            }

            logger.LogInformation("Creating backtest with ID: {id} for user {userId}", request.Id, request.UserId);

            var parameters = new BacktestRequestDetails
            {
                PositionInfo = request.PositionInfo,
                Exit = request.Exit,
                Argument = request.Argument
            };

            var record = new BacktestRecord
            {
                Id = request.Id,
                UserId = request.UserId,
                Status = BacktestStatus.Pending,
                CreatedAt = DateTimeOffset.Now.ToString(),
                Start = request.Start.ToString("yyyy-MM-dd"),
                End = request.End.ToString("yyyy-MM-dd"),
                RequestDetails = BacktestUtilities.CompressRequestDetails(parameters)
            };

            await repository.Put(record, null);

            var json = JsonSerializer.Serialize(request);

           _ = lambda.InvokeAsync(new InvokeRequest
            {
                FunctionName = config.LambdaName,
                Payload = json
            });

            return new OperationResult<BacktestEntryResponse>
            {
                Status = HttpStatusCode.OK,
                Data = new BacktestEntryResponse
                {
                    Id = request.Id,
                    Status = BacktestStatus.Pending,
                    CreatedAt = record.CreatedAt,
                    RequestDetails = parameters,
                }
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating backtest with ID: {id}", request.Id);
            return new OperationResult<BacktestEntryResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal error."]
            };
        }
    }

    public async Task<OperationResult<List<BacktestEntryResponse>>> List(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new OperationResult<List<BacktestEntryResponse>>
                {
                    Status = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Invalid user ID."]
                };
            }

            var records = await repository.List(userId);

            List<BacktestEntryResponse> entries = [];
            foreach (var record in records)
            {
                entries.Add(new BacktestEntryResponse
                {
                    Id = record.Id,
                    Status = record.Status,
                    CreatedAt = record.CreatedAt,
                    CreditsUsed = record.CreditsUsed,
                    HoldProfit = record.HoldProfit,
                    HighProfit = record.HighProfit,
                    OtherProfit = record.OtherProfit,
                    Start = record.Start,
                    End = record.End,
                    RequestDetails = BacktestUtilities.DecompressRequestDetails(record.RequestDetails),
                    Errors = record.Errors
                });
            }

            return new OperationResult<List<BacktestEntryResponse>>
            {
                Status = HttpStatusCode.OK,
                Data = entries
            };
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing backtests for user: {userId}", userId);
            return new OperationResult<List<BacktestEntryResponse>>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal error."]
            };
        }
    }

    public async Task<OperationResult<BacktestEntryResponse>> GetEntry(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new OperationResult<BacktestEntryResponse>
                {
                    Status = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Invalid backtest ID."]
                };
            }

            logger.LogInformation("Getting backtest entry for ID: {id}", id);

            var record = await repository.Get(id);

            if (record is null)
            {
                return new OperationResult<BacktestEntryResponse>
                {
                    Status = HttpStatusCode.NotFound,
                    ErrorMessages = ["Backtest not found."]
                };
            }

            return new OperationResult<BacktestEntryResponse>
            {
                Status = HttpStatusCode.OK,
                Data = new BacktestEntryResponse
                {
                    Id = record.Id,
                    Status = record.Status,
                    CreatedAt = record.CreatedAt,
                    CreditsUsed = record.CreditsUsed,
                    HoldProfit = record.HoldProfit,
                    HighProfit = record.HighProfit,
                    OtherProfit = record.OtherProfit,
                    Start = record.Start,
                    End = record.End,
                    RequestDetails = BacktestUtilities.DecompressRequestDetails(record.RequestDetails),
                    Errors = record.Errors
                }
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting backtest entry for ID: {id}", id);
            return new OperationResult<BacktestEntryResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal error."]
            };
        }
    }

    public async Task<OperationResult<BacktestResultResponse>> GetResult(string id)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new OperationResult<BacktestResultResponse>
                {
                    Status = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Invalid backtest ID."]
                };
            }

            logger.LogInformation("Getting backtest result for ID: {id}", id);

            var record = await repository.Get(id);

            if (record is null)
            {
                return new OperationResult<BacktestResultResponse>
                {
                    Status = HttpStatusCode.NotFound,
                    ErrorMessages = ["Backtest not found."]
                };
            }

            if (record.Status != BacktestStatus.Completed)
            {
                return new OperationResult<BacktestResultResponse>
                {
                    Status = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Backtest is not completed yet."]
                };
            }

            var parameters = BacktestUtilities.DecompressRequestDetails(record.RequestDetails);

            var entries = await repository.GetBacktestResultsFromS3(record);

            if (entries is null || !entries.Any())
            {
                return new OperationResult<BacktestResultResponse>
                {
                    Status = HttpStatusCode.NotFound,
                    ErrorMessages = ["No results found for this backtest."]
                };
            }

            var availableFundsHold = parameters.PositionInfo.StartingBalance;
            var availableFundsOther = parameters.PositionInfo.StartingBalance;
            var availableFundsHigh = parameters.PositionInfo.StartingBalance;

            var holdOpenPositions = new List<BacktestEntryResultCollection>();
            var otherOpenPositions = new List<BacktestEntryResultCollection>();
            var highOpenPositions = new List<BacktestEntryResultCollection>();

            int maxConcurrentHoldPositions = 0;
            int maxConcurrentHighPositions = 0;
            int maxConcurrentOtherPositions = 0;

            var dayRange = GetDateRange(record, entries);

            var backtestDayResults = new List<BacktestDayResultV3>();

            foreach (var day in dayRange)
            {
                var offset = TimeZone.IsDaylightSavingTime(day.Date) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
                var marketOpen = new DateTimeOffset(day.Year, day.Month, day.Day, 9, 30, 0, offset);
                var marketClose = new DateTimeOffset(day.Year, day.Month, day.Day, 16, 0, 0, offset);

                var entry = entries.FirstOrDefault(q => q.Date.ToString("yyyy-MM-dd") == day.ToString("yyyy-MM-dd"));

                var backtestEntryDay = new BacktestDayResultV3
                {
                    Date = day,
                    Hold = new BacktestDayDetails
                    {
                        StartCashAvailable = availableFundsHold,
                        Bought = [],
                        Sold = []
                    },
                    High = new BacktestDayDetails
                    {
                        StartCashAvailable = availableFundsHigh,
                        Bought = [],
                        Sold = []
                    },
                    Other = parameters.Exit.Other is null ? null : new BacktestDayDetails
                    {
                        StartCashAvailable = availableFundsOther,
                        Bought = [],
                        Sold = []
                    }
                };

                for (int i = 0; i < (marketClose - marketOpen).TotalMinutes; i++)
                {
                    var currentTime = marketOpen.AddMinutes(i);

                    SellPositionIfApplicable("hold", holdOpenPositions, currentTime, ref availableFundsHold, backtestEntryDay);
                    if (parameters.Exit.Other is not null)
                    {
                        SellPositionIfApplicable("other", otherOpenPositions, currentTime, ref availableFundsOther, backtestEntryDay);
                    }
                    SellPositionIfApplicable("high", highOpenPositions, currentTime, ref availableFundsHigh, backtestEntryDay);

                    BuyPositionIfApplicable("hold", entry, currentTime, parameters, ref availableFundsHold, holdOpenPositions, backtestEntryDay);
                    if (parameters.Exit.Other is not null)
                    {
                        BuyPositionIfApplicable("other", entry, currentTime, parameters, ref availableFundsOther, otherOpenPositions, backtestEntryDay);
                    }
                    BuyPositionIfApplicable("high", entry, currentTime, parameters, ref availableFundsHigh, highOpenPositions, backtestEntryDay);

                    maxConcurrentHoldPositions = holdOpenPositions.Count > maxConcurrentHoldPositions ? holdOpenPositions.Count : maxConcurrentHoldPositions;
                    maxConcurrentHighPositions = highOpenPositions.Count > maxConcurrentHighPositions ? highOpenPositions.Count : maxConcurrentHighPositions;
                    if (parameters.Exit.Other is not null)
                    {
                        maxConcurrentOtherPositions = otherOpenPositions.Count > maxConcurrentHighPositions ? otherOpenPositions.Count : maxConcurrentHighPositions;
                    }
                }

                backtestEntryDay.Hold.EndCashAvailable = availableFundsHold;
                backtestEntryDay.Hold.TotalBalance = holdOpenPositions.Sum(q => q.StartPosition) + backtestEntryDay.Hold.EndCashAvailable;

                if (parameters.Exit.Other is not null)
                {
                    backtestEntryDay.Other.EndCashAvailable = availableFundsOther;
                    backtestEntryDay.Other.TotalBalance = otherOpenPositions.Sum(q => q.StartPosition) + backtestEntryDay.Other.EndCashAvailable;
                }

                backtestEntryDay.High.EndCashAvailable = availableFundsHigh;
                backtestEntryDay.High.TotalBalance = highOpenPositions.Sum(q => q.StartPosition) + backtestEntryDay.High.EndCashAvailable;

                backtestDayResults.Add(backtestEntryDay);
            }

            var holdWins = backtestDayResults.SelectMany(q => q.Hold.Sold).Where(q => q.Profit > 0);
            var holdLosses = backtestDayResults.SelectMany(q => q.Hold.Sold).Where(q => q.Profit < 0);

            var otherWins = parameters.Exit.Other is null ? [] : backtestDayResults.SelectMany(q => q.Other.Sold).Where(q => q.Profit > 0);
            var otherLosses = parameters.Exit.Other is null ? [] : backtestDayResults.SelectMany(q => q.Other.Sold).Where(q => q.Profit < 0);

            var highWins = backtestDayResults.SelectMany(q => q.High.Sold).Where(q => q.Profit > 0);
            var highLosses = backtestDayResults.SelectMany(q => q.High.Sold).Where(q => q.Profit < 0);

            return new OperationResult<BacktestResultResponse>
            {
                Status = HttpStatusCode.OK,
                Data = new BacktestResultResponse
                {
                    Id = record.Id,
                    Hold = new BacktestEntryStats
                    {
                        EndBalance = availableFundsHold,
                        SumProfit = availableFundsHold - parameters.PositionInfo.StartingBalance,
                        WinRatio = holdWins.Any() ? (float)holdWins.Count() / (float)(holdWins.Count() + holdLosses.Count()) : 0,
                        AvgWin = holdWins.Any() ? holdWins.Average(q => q.Profit) : 0,
                        AvgLoss = holdLosses.Any() ? holdLosses.Average(q => q.Profit) : 0,
                        MaxConcurrentPositions = backtestDayResults.Any() ? backtestDayResults.Max(result => result.Hold.OpenPositions) : 0
                    },
                    High = new BacktestEntryStats
                    {
                        EndBalance = availableFundsHigh,
                        SumProfit = availableFundsHigh - parameters.PositionInfo.StartingBalance,
                        WinRatio = highWins.Any() ? (float)highWins.Count() / (float)(highWins.Count() + highLosses.Count()) : 0,
                        AvgWin = highWins.Any() ? highWins.Average(q => q.Profit) : 0,
                        AvgLoss = highLosses.Any() ? highLosses.Average(q => q.Profit) : 0,
                        MaxConcurrentPositions = backtestDayResults.Any() ? backtestDayResults.Max(result => result.High.OpenPositions) : 0
                    },
                    Other = parameters.Exit.Other is null ? null : new BacktestEntryStats
                    {
                        EndBalance = availableFundsOther,
                        SumProfit = availableFundsOther - parameters.PositionInfo.StartingBalance,
                        WinRatio = otherWins.Any() ? (float)otherWins.Count() / (float)(otherWins.Count() + otherLosses.Count()) : 0,
                        AvgWin = otherWins.Any() ? otherWins.Average(q => q.Profit) : 0,
                        AvgLoss = otherLosses.Any() ? otherLosses.Average(q => q.Profit) : 0,
                        MaxConcurrentPositions = backtestDayResults.Any() ? backtestDayResults.Max(result => result.Other.OpenPositions) : 0
                    },
                    Results = backtestDayResults,
                    Entries = entries,
                }
            };

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting backtest result for ID: {id}", id);
            return new OperationResult<BacktestResultResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal error."]
            };
        }
    }

    #region Private Methods

    private static IEnumerable<DateTimeOffset> GetDateRange(BacktestRecord record, IEnumerable<WorkerResponse> entries)
    {
        var entriesWithDates = entries.Where(q => q.Results.Any());

        if (!entriesWithDates.Any())
        {
            return [];
        }

        var lastDates = new List<DateTimeOffset>
        {
            entriesWithDates.Max(q => q.Results.Max(result => result.Hold.SoldAt)),
            entriesWithDates.Max(q => q.Results.Max(result => result.High.SoldAt)),
        };

        var otherDates = entriesWithDates.Where(q => q.Other is not null);
        if (otherDates.Any())
        {
            lastDates.Add(otherDates.Max(q => q.Results.Max(result => result.Other.SoldAt)));
        }

        if (!lastDates.Any())
        {
            return [];
        }

        var maxDate = lastDates.Max();
        var startDate = DateTimeOffset.Parse(record.Start);

        return Enumerable.Range(0, (maxDate - startDate).Days + 1)
            .Select(day => startDate.AddDays(day))
            .Where(day => day.DayOfWeek != DayOfWeek.Sunday && day.DayOfWeek != DayOfWeek.Saturday);
    }

    private static void SellPositionIfApplicable(
        string type,
        List<BacktestEntryResultCollection> openPositions,
        DateTimeOffset timestamp,
        ref float availableFunds,
        BacktestDayResultV3 backtestDay)
    {
        List<BacktestEntryResultCollection> positionsToRemove = [];

        var positionsToSell = type.ToLowerInvariant() switch
        {
            "hold" => openPositions.Where(position => position.Hold.SoldAt == timestamp),
            "high" => openPositions.Where(position => position.High.SoldAt == timestamp),
            "other" => openPositions.Where(position => position.Other.SoldAt == timestamp),
            _ => throw new NotImplementedException()
        };

        foreach (var position in positionsToSell)
        {
            availableFunds += type.ToLowerInvariant() switch
            {
                "hold" => position.Hold.EndPosition,
                "high" => position.High.EndPosition,
                "other" => position.Other.EndPosition,
                _ => throw new NotImplementedException()
            };
            positionsToRemove.Add(position);
        }

        foreach (var position in positionsToRemove)
        {
            switch (type.ToLowerInvariant())
            {
                case "hold":
                    backtestDay.Hold.Sold.Add(new BacktestDayPosition
                    {
                        Ticker = position.Ticker,
                        Price = position.Hold.EndPrice,
                        Shares = position.Shares,
                        Position = position.Hold.EndPosition,
                        Profit = position.Hold.Profit,
                        Timestamp = position.Hold.SoldAt,
                        StoppedOut = position.Hold.StoppedOut
                    });
                    break;

                case "high":
                    backtestDay.High.Sold.Add(new BacktestDayPosition
                    {
                        Ticker = position.Ticker,
                        Price = position.High.EndPrice,
                        Shares = position.Shares,
                        Position = position.High.EndPosition,
                        Profit = position.High.Profit,
                        Timestamp = position.High.SoldAt,
                        StoppedOut = position.High.StoppedOut
                    });
                    break;

                case "other":
                    backtestDay.Other.Sold.Add(new BacktestDayPosition
                    {
                        Ticker = position.Ticker,
                        Price = position.Other.EndPrice,
                        Shares = position.Shares,
                        Position = position.Other.EndPosition,
                        Profit = position.Other.Profit,
                        Timestamp = position.Other.SoldAt,
                        StoppedOut = position.Other.StoppedOut
                    });
                    break;

                default: throw new NotImplementedException();
            };

            openPositions.Remove(position);
        }
    }

    private static void BuyPositionIfApplicable(
        string type,
        WorkerResponse entry,
        DateTimeOffset timestamp,
        BacktestRequestDetails parameters,
        ref float availableFunds,
        List<BacktestEntryResultCollection> openPositions,
        BacktestDayResultV3 backtestDay)
    {
        if (entry is null)
        {
            return;
        }

        var results = entry.Results.Where(result => result.BoughtAt == timestamp);

        //var feature = parameters.Features?.FirstOrDefault(q => q.Type == FeatureType.TickerType);

        foreach (var result in results)
        {
            if (availableFunds < parameters.PositionInfo.PositionSize || openPositions.Count >= parameters.PositionInfo.MaxConcurrentPositions)
            {
                continue;
            }

            var backtestDayPosition = new BacktestDayPosition
            {
                Ticker = result.Ticker,
                Price = result.StartPrice,
                Shares = result.Shares,
                Position = result.StartPosition,
                Timestamp = result.BoughtAt,
            };

            switch (type.ToLowerInvariant())
            {
                case "hold":
                    backtestDay.Hold.Bought.Add(backtestDayPosition);
                    break;
                case "high":
                    backtestDay.High.Bought.Add(backtestDayPosition);
                    break;
                case "other":
                    backtestDay.Other.Bought.Add(backtestDayPosition);
                    break;
                default:
                    throw new NotImplementedException();
            }

            openPositions.Add(result);
            availableFunds -= result.StartPosition;
        }
    }

    #endregion
}
