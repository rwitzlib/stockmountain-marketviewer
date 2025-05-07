using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;
using FluentValidation;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Entities.Backtest;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Presentation.Requests.Backtest;
using MarketViewer.Contracts.Presentation.Responses.Backtest;
using MarketViewer.Core.Models;
using MarketViewer.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MarketViewer.Application.Handlers.Backtest;

public class BacktestHandlerV3(
    IDynamoDBContext _dbContext,
    IAmazonS3 _s3Client,
    BacktestService _backtestService,
    IMarketCache _marketCache,
    ILogger<BacktestHandlerV3> _logger) : IRequestHandler<BacktestRequestV3, OperationResult<BacktestResponseV3>>
{
    private readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
    private const int ESTIMATED_DAILY_CREDIT_COST = 120; // Estimated Credit Cost per Day

    public async Task<OperationResult<BacktestResponseV3>> Handle(BacktestRequestV3 request, CancellationToken cancellationToken)
    {
        try
        {
            var estimatedCreditCost = ((request.End - request.Start).Days + 1) * ESTIMATED_DAILY_CREDIT_COST;
            var user = await _dbContext.LoadAsync<User>(request.UserId, cancellationToken);
            if (user == null || user.Credits < estimatedCreditCost)
            {
                return GenerateErrorResponse(HttpStatusCode.Forbidden, ["Insufficient credits."]);
            }

            List<BacktestLambdaResponseV3> entries = [];
            List<BacktestLambdaResponseV3> s3Entries = [];

            if (_backtestService.CheckForBacktestHistory(CompressRequestDetails(request), out var record))
            {
                s3Entries = await _backtestService.GetBacktestResultsFromS3(record);

                var startDate = s3Entries.Min(q => q.Date);
                var endDate = s3Entries.Max(q => q.Date);

                if (request.Start.Date < startDate)
                {
                    var missingEntries = await _backtestService.GetBacktestResultsFromLambda(new BacktestRequestV3
                    {
                        Id = request.Id,
                        Start = request.Start,
                        End = startDate.AddDays(-1),
                        PositionInfo = request.PositionInfo,
                        Exit = request.Exit,
                        Features = request.Features,
                        Argument = request.Argument
                    });
                    entries.AddRange(missingEntries);
                }

                entries.AddRange(s3Entries);

                if (request.End.Date > endDate)
                {
                    var missingEntries = await _backtestService.GetBacktestResultsFromLambda(new BacktestRequestV3
                    {
                        Id = request.Id,
                        Start = endDate.AddDays(1),
                        End = request.End,
                        PositionInfo = request.PositionInfo,
                        Exit = request.Exit,
                        Features = request.Features,
                        Argument = request.Argument
                    });
                    entries.AddRange(missingEntries);
                }
            }
            else
            {
                entries = await _backtestService.GetBacktestResultsFromLambda(request);
            }

            var relevantEntries = entries.Where(q => q.Date >= request.Start && q.Date <= request.End);

            if (relevantEntries is null || !relevantEntries.Any())
            {
                return GenerateErrorResponse(HttpStatusCode.NotFound, ["No results."]);
            }

            var availableFundsHold = request.PositionInfo.StartingBalance;
            var availableFundsOther = request.PositionInfo.StartingBalance;
            var availableFundsHigh = request.PositionInfo.StartingBalance;

            var holdOpenPositions = new List<BacktestEntryResultCollection>();
            var otherOpenPositions = new List<BacktestEntryResultCollection>();
            var highOpenPositions = new List<BacktestEntryResultCollection>();

            int maxConcurrentHoldPositions = 0;
            int maxConcurrentHighPositions = 0;
            int maxConcurrentOtherPositions = 0;

            var dayRange = GetDateRange(request, relevantEntries);

            var backtestDayResults = new List<BacktestDayResultV3>();

            foreach (var day in dayRange)
            {
                var offset = TimeZone.IsDaylightSavingTime(day.Date) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
                var marketOpen = new DateTimeOffset(day.Year, day.Month, day.Day, 9, 30, 0, offset);
                var marketClose = new DateTimeOffset(day.Year, day.Month, day.Day, 16, 0, 0, offset);

                var entry = relevantEntries.FirstOrDefault(q => q.Date == day.Date);

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
                    Other = request.Exit.Other is null ? null : new BacktestDayDetails
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
                    if (request.Exit.Other is not null)
                    {
                        SellPositionIfApplicable("other", otherOpenPositions, currentTime, ref availableFundsOther, backtestEntryDay);
                    }
                    SellPositionIfApplicable("high", highOpenPositions, currentTime, ref availableFundsHigh, backtestEntryDay);

                    BuyPositionIfApplicable("hold", entry, currentTime, request, ref availableFundsHold, holdOpenPositions, backtestEntryDay);
                    if (request.Exit.Other is not null)
                    {
                        BuyPositionIfApplicable("other", entry, currentTime, request, ref availableFundsOther, otherOpenPositions, backtestEntryDay);
                    }
                    BuyPositionIfApplicable("high", entry, currentTime, request, ref availableFundsHigh, highOpenPositions, backtestEntryDay);

                    maxConcurrentHoldPositions = holdOpenPositions.Count > maxConcurrentHoldPositions ? holdOpenPositions.Count : maxConcurrentHoldPositions;
                    maxConcurrentHighPositions = highOpenPositions.Count > maxConcurrentHighPositions ? highOpenPositions.Count : maxConcurrentHighPositions;
                    if (request.Exit.Other is not null)
                    {
                        maxConcurrentOtherPositions = otherOpenPositions.Count > maxConcurrentHighPositions ? otherOpenPositions.Count : maxConcurrentHighPositions;
                    }
                }

                backtestEntryDay.Hold.EndCashAvailable = availableFundsHold;
                backtestEntryDay.Hold.TotalBalance = holdOpenPositions.Sum(q => q.StartPosition) + backtestEntryDay.Hold.EndCashAvailable;

                if (request.Exit.Other is not null)
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

            var otherWins = request.Exit.Other is null ? [] : backtestDayResults.SelectMany(q => q.Other.Sold).Where(q => q.Profit > 0);
            var otherLosses = request.Exit.Other is null ? [] : backtestDayResults.SelectMany(q => q.Other.Sold).Where(q => q.Profit < 0);

            var highWins = backtestDayResults.SelectMany(q => q.High.Sold).Where(q => q.Profit > 0);
            var highLosses = backtestDayResults.SelectMany(q => q.High.Sold).Where(q => q.Profit < 0);

            var response = new OperationResult<BacktestResponseV3>
            {
                Status = HttpStatusCode.OK,
                Data = new BacktestResponseV3
                {
                    Id = request.Id,
                    CreditsUsed = relevantEntries.Where(result => result is not null).Sum(result => result.CreditsUsed),
                    Hold = new BacktestEntryStats
                    {
                        EndBalance = availableFundsHold,
                        BalanceChange = (availableFundsHold - request.PositionInfo.StartingBalance) / request.PositionInfo.StartingBalance,
                        SumProfit = availableFundsHold - request.PositionInfo.StartingBalance,
                        WinRatio = holdWins.Any() ? (float)holdWins.Count() / (float)(holdWins.Count() + holdLosses.Count()) : 0,
                        AvgWin = holdWins.Any() ? holdWins.Average(q => q.Profit) : 0,
                        AvgLoss = holdLosses.Any() ? holdLosses.Average(q => q.Profit) : 0,
                        MaxConcurrentPositions = maxConcurrentHoldPositions
                    },
                    High = new BacktestEntryStats
                    {
                        EndBalance = availableFundsHigh,
                        BalanceChange = (availableFundsHigh - request.PositionInfo.StartingBalance) / request.PositionInfo.StartingBalance,
                        SumProfit = availableFundsHigh - request.PositionInfo.StartingBalance,
                        WinRatio = highWins.Any() ? (float)highWins.Count() / (float)(highWins.Count() + highLosses.Count()) : 0,
                        AvgWin = highWins.Any() ? highWins.Average(q => q.Profit) : 0,
                        AvgLoss = highLosses.Any() ? highLosses.Average(q => q.Profit) : 0,
                        MaxConcurrentPositions = maxConcurrentHighPositions
                    },
                    Other = request.Exit.Other is null ? null : new BacktestEntryStats
                    {
                        EndBalance = availableFundsOther,
                        BalanceChange = (availableFundsOther - request.PositionInfo.StartingBalance) / request.PositionInfo.StartingBalance,
                        SumProfit = availableFundsOther - request.PositionInfo.StartingBalance,
                        WinRatio = otherWins.Any() ? (float)otherWins.Count() / (float)(otherWins.Count() + otherLosses.Count()) : 0,
                        AvgWin = otherWins.Any() ? otherWins.Average(q => q.Profit) : 0,
                        AvgLoss = otherLosses.Any() ? otherLosses.Average(q => q.Profit) : 0,
                        MaxConcurrentPositions = maxConcurrentOtherPositions
                    },
                    Results = backtestDayResults,
                    Entries = relevantEntries
                }
            };

            var newRecord = new BacktestRecord
            {
                Id = request.Id,
                CustomerId = Guid.Empty.ToString(),
                CreatedAt = DateTimeOffset.Now.ToString("yyyy-MM-ddThh:mmZ"),
                CreditsUsed = relevantEntries.Where(result => result is not null).Sum(result => result.CreditsUsed),
                S3ObjectName = record is null ? Guid.NewGuid().ToString() : record.S3ObjectName,
                HoldProfit = response.Data.Hold.SumProfit,
                HighProfit = response.Data.High.SumProfit,
                RequestDetails = CompressRequestDetails(request)
            };

            await _dbContext.SaveAsync(newRecord, cancellationToken);

            user.Credits -= response.Data.CreditsUsed;
            await _dbContext.SaveAsync(user, cancellationToken);

            if (s3Entries.Count < entries.Count)
            {
                var s3Response = await _s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = "lad-dev-marketviewer",
                    Key = $"backtestResults/{newRecord.S3ObjectName}",
                    ContentBody = JsonSerializer.Serialize(entries)
                }, cancellationToken);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: {}", ex.Message);
            _logger.LogError("Stacktrace: {}", ex.StackTrace);

            return new OperationResult<BacktestResponseV3>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = [ex.Message]
            };
        }
    }

    private static string CompressRequestDetails(BacktestRequestV3 request)
    {
        var requestDetails = $"{JsonSerializer.Serialize(request.PositionInfo)}{JsonSerializer.Serialize(request.Exit)}{JsonSerializer.Serialize(request.Argument)}";
        var bytes = Encoding.UTF8.GetBytes(requestDetails);
        using var outputStream = new MemoryStream();
        using (var compressionStream = new GZipStream(outputStream, CompressionMode.Compress))
        {
            compressionStream.Write(bytes, 0, bytes.Length);
        }
        return Convert.ToBase64String(outputStream.ToArray());
    }

    private static OperationResult<BacktestResponseV3> GenerateErrorResponse(HttpStatusCode status, IEnumerable<string> errors)
    {
        return new OperationResult<BacktestResponseV3>
        {
            Status = status,
            ErrorMessages = errors
        };
    }

    private static IEnumerable<DateTimeOffset> GetDateRange(BacktestRequestV3 request, IEnumerable<BacktestLambdaResponseV3> entries)
    {
        var entriesWithDates = entries.Where(q => q.Results.Any());

        if (!entriesWithDates.Any())
        {
            return [];
        }

        var lastDate = new DateTimeOffset[]
        {
            entriesWithDates.Max(q => q.Results.Max(result => result.Hold.SoldAt)),
            entriesWithDates.Max(q => q.Results.Max(result => result.High.SoldAt)),
            entriesWithDates.Any(q => q.Other is not null) ? entriesWithDates.Max(q => q.Results.Where(q => q.Other is not null).Max(result => result.Other.SoldAt)) : DateTimeOffset.MinValue
        };       

        if (lastDate.Length <= 0)
        {
            return [];
        }

        var maxDate = lastDate.Max();
        var startDate = request.Start;

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

    private void BuyPositionIfApplicable(
        string type,
        BacktestLambdaResponseV3 entry,
        DateTimeOffset timestamp,
        BacktestRequestV3 request,
        ref float availableFunds,
        List<BacktestEntryResultCollection> openPositions,
        BacktestDayResultV3 backtestDay)
    {
        if (entry is null)
        {
            return;
        }

        var results = entry.Results.Where(result => result.BoughtAt == timestamp);

        var feature = request.Features?.FirstOrDefault(q => q.Type == FeatureType.TickerType);

        foreach (var result in results)
        {
            var tickerDetails = _marketCache.GetTickerDetails(result.Ticker);

            if (tickerDetails is null || (feature != null && !feature.Value.Contains(tickerDetails.Type)) ||
                availableFunds < request.PositionInfo.PositionSize || openPositions.Count >= request.PositionInfo.MaxConcurrentPositions)
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
}
