using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;

using FluentValidation;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Requests.Backtest;
using MarketViewer.Contracts.Responses.Backtest;
using MarketViewer.Infrastructure.Services;

using MediatR;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MarketViewer.Application.Handlers.Backtest;

public class BacktestHandlerV3(
    IDynamoDBContext _dbContext,
    IAmazonS3 _s3Client,
    BacktestService _backtestService,
    MemoryMarketCache _marketCache,
    ILogger<BacktestHandlerV3> _logger) : IRequestHandler<BacktestRequestV3, OperationResult<BacktestResponseV3>>
{
    private readonly TimeZoneInfo _marketTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");

    public async Task<OperationResult<BacktestResponseV3>> Handle(BacktestRequestV3 request, CancellationToken cancellationToken)
    {
        try
        {
            List<BacktestLambdaResponseV3> entries = [];

            //if (_backtestService.CheckForBacktestHistory(request, out var record))
            //{
            //    entries = await _backtestService.GetBacktestResultsFromS3(record);
            //}

            if (entries.Count <= 0)
            {
                entries = await _backtestService.GetBacktestResultsFromLambda(request);
            }

            var validEntries = entries.Where(q => q.Date >= request.Start && q.Date <= request.End);

            if (validEntries is null || !validEntries.Any())
            {
                return GenerateErrorResponse(HttpStatusCode.NotFound, ["No results."]);
            }

            var availableFundsHold = request.PositionInfo.StartingBalance;
            var availableFundsOther = request.PositionInfo.StartingBalance;
            var availableFundsHigh = request.PositionInfo.StartingBalance;

            var holdOpenPositions = new List<BacktestEntryResultCollection>();
            var otherOpenPositions = new List<BacktestEntryResultCollection>();
            var highOpenPositions = new List<BacktestEntryResultCollection>();

            var dayRange = GetDateRange(request, validEntries);

            var backtestDayResults = new List<BacktestDayResultV3>();

            foreach (var day in dayRange)
            {
                var offset = _marketTimeZone.IsDaylightSavingTime(day.Date) ? TimeSpan.FromHours(-4) : TimeSpan.FromHours(-5);
                var marketOpen = new DateTimeOffset(day.Year, day.Month, day.Day, 9, 30, 0, offset);
                var marketClose = new DateTimeOffset(day.Year, day.Month, day.Day, 16, 0, 0, offset);

                var entry = validEntries.FirstOrDefault(q => q.Date == day.Date);

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

                    if (day.Date.ToString("yyyy-MM-dd") == "2024-10-05")
                    {
                        if (currentTime.ToString("hh:mm") == "09:31")
                        {

                        }
                    }


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
                }

                backtestEntryDay.Hold.OpenPositions = holdOpenPositions.Count;
                backtestEntryDay.Hold.EndCashAvailable = availableFundsHold;
                backtestEntryDay.Hold.TotalBalance = holdOpenPositions.Sum(q => q.StartPosition) + backtestEntryDay.Hold.EndCashAvailable;

                if (request.Exit.Other is not null)
                {
                    backtestEntryDay.Other.OpenPositions = otherOpenPositions.Count;
                    backtestEntryDay.Other.EndCashAvailable = availableFundsOther;
                    backtestEntryDay.Other.TotalBalance = otherOpenPositions.Sum(q => q.StartPosition) + backtestEntryDay.Other.EndCashAvailable;
                }

                backtestEntryDay.High.OpenPositions = highOpenPositions.Count;
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
                    Hold = new BacktestEntryStats
                    {
                        EndBalance = availableFundsHold,
                        SumProfit = availableFundsHold - request.PositionInfo.StartingBalance,
                        PositiveTrendRatio = (float)holdWins.Count() / (float)(holdWins.Count() + holdLosses.Count()),
                        AvgWin = holdWins.Any() ? holdWins.Average(q => q.Profit) : 0,
                        AvgLoss = holdLosses.Any() ? holdLosses.Average(q => q.Profit) : 0,
                        //AvgProfit = backtestDayResults.SelectMany(q => q.Hold.Sold).Average(q => q.Profit),
                        MaxConcurrentPositions = backtestDayResults.Max(result => result.Hold.OpenPositions)
                    },
                    High = new BacktestEntryStats
                    {
                        EndBalance = availableFundsHigh,
                        SumProfit = availableFundsHigh - request.PositionInfo.StartingBalance,
                        PositiveTrendRatio = (float)highWins.Count() / (float)(highWins.Count() + highLosses.Count()),
                        AvgWin = highWins.Any() ? highWins.Average(q => q.Profit) : 0,
                        AvgLoss = highLosses.Any() ? highLosses.Average(q => q.Profit) : 0,
                        //AvgProfit = backtestDayResults.SelectMany(q => q.High.Sold).Average(q => q.Profit),
                        MaxConcurrentPositions = backtestDayResults.Max(result => result.High.OpenPositions)
                    },
                    Other = request.Exit.Other is null ? null : new BacktestEntryStats
                    {
                        EndBalance = availableFundsOther,
                        SumProfit = availableFundsOther - request.PositionInfo.StartingBalance,
                        PositiveTrendRatio = (float)otherWins.Count() / (float)(otherWins.Count() + otherLosses.Count()),
                        AvgWin = otherWins.Any() ? otherWins.Average(q => q.Profit) : 0,
                        AvgLoss = otherLosses.Any() ? otherLosses.Average(q => q.Profit) : 0,
                        //AvgProfit = backtestDayResults.SelectMany(q => q.Other.Sold).Average(q => q.Profit),
                        MaxConcurrentPositions = backtestDayResults.Max(result => result.Other.OpenPositions)
                    },
                    Results = backtestDayResults,
                    Entries = validEntries
                }
            };

            var newRecord = new BacktestRecord
            {
                Id = request.Id,
                CustomerId = Guid.Empty.ToString(),
                CreatedAt = DateTimeOffset.Now.ToString("yyyy-MM-dd hh:mm z"),
                CreditsUsed = validEntries.Where(result => result is not null).Sum(result => result.CreditsUsed),
                HoldProfit = response.Data.Hold.SumProfit,
                HighProfit = response.Data.High.SumProfit,
                //RequestDetails = $"{JsonSerializer.Serialize(request.Exit)}{JsonSerializer.Serialize(request.Argument)}",
                StartDate = int.Parse(request.Start.ToString("yyyyMMdd")),
                EndDate = int.Parse(request.End.ToString("yyyyMMdd")),
                //S3ObjectName = record is null ? Guid.NewGuid().ToString() : record.S3ObjectName
            };

            await _dbContext.SaveAsync(newRecord, cancellationToken);

            //if (record is null)
            //{
            //    var s3Response = await _s3Client.PutObjectAsync(new PutObjectRequest
            //    {
            //        BucketName = "lad-dev-marketviewer",
            //        Key = $"backtestResults/{newRecord.S3ObjectName}",
            //        ContentBody = JsonSerializer.Serialize(validEntries)
            //    }, cancellationToken);
            //}

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
        DateTimeOffset[] lastDate = [
            entries.Max(q => q.Results.Max(result => result.Hold.SoldAt)),
            entries.Max(q => q.Results.Max(result => result.High.SoldAt)),
            entries.Max(q => q.Results.Max(result => result.Other.SoldAt))
        ];

        return Enumerable.Range(0, (lastDate.Max() - request.Start).Days + 1)
            .Select(day => request.Start.AddDays(day))
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

        if (timestamp.Date.ToString("yyyy-MM-dd") == "2024-09-04")
        {

        }
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

        var feature = (request.Features is not null && request.Features.Any()) 
            ? request.Features.FirstOrDefault(q => q.Type == FeatureType.TickerType) : null;

        foreach (var result in results)
        {
            var tickerDetails = _marketCache.GetTickerDetails(result.Ticker);

            if (tickerDetails is null)
            {
                continue;
            }

            if (feature is not null && !feature.Value.Contains(tickerDetails.Type))
            {
                continue;
            }

            if (availableFunds < request.PositionInfo.PositionSize || openPositions.Count >= request.PositionInfo.MaxConcurrentPositions)
            {
                continue;
            }

            switch (type.ToLowerInvariant())
            {
                case "hold":
                    {
                        backtestDay.Hold.Bought.Add(new BacktestDayPosition
                        {
                            Ticker = result.Ticker,
                            Price = result.StartPrice,
                            Shares = result.Shares,
                            Position = result.StartPosition,
                            Timestamp = result.BoughtAt,
                        });
                    }
                    break;

                case "high":
                    {
                        backtestDay.High.Bought.Add(new BacktestDayPosition
                        {
                            Ticker = result.Ticker,
                            Price = result.StartPrice,
                            Shares = result.Shares,
                            Position = result.StartPosition,
                            Timestamp = result.BoughtAt,
                        });
                    }
                    break;

                case "other":
                    {
                        backtestDay.Other.Bought.Add(new BacktestDayPosition
                        {
                            Ticker = result.Ticker,
                            Price = result.StartPrice,
                            Shares = result.Shares,
                            Position = result.StartPosition,
                            Timestamp = result.BoughtAt,
                        });
                    }
                    break;

                default:
                    throw new NotImplementedException();
            }

            openPositions.Add(result);
            availableFunds -= result.StartPosition;
        }
    }
}
