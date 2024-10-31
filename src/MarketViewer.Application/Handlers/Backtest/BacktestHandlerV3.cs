using Amazon.DynamoDBv2.DataModel;
using Amazon.S3;
using Amazon.S3.Model;

using FluentValidation;
using MarketViewer.Contracts.Caching;
using MarketViewer.Contracts.Enums.Backtest;
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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MarketViewer.Application.Handlers.Backtest;

public class BacktestHandlerV3(
    IDynamoDBContext _dbContext,
    IAmazonS3 _s3Client,
    BacktestService _backtestService,
    MarketCache _marketCache,
    ILogger<BacktestHandlerV3> _logger) : IRequestHandler<BacktestV3Request, OperationResult<BacktestV3Response>>
{
    public async Task<OperationResult<BacktestV3Response>> Handle(BacktestV3Request request, CancellationToken cancellationToken)
    {
        try
        {
            List<BacktestEntryV3> entries = [];

            if (_backtestService.CheckForBacktestHistory(request, out var record))
            {
                entries = await _backtestService.GetBacktestResultsFromS3(record);
            }

            if (entries.Count <= 0)
            {
                entries = await _backtestService.GetBacktestResultsFromLambda(request);
            }

            var validEntries = entries.Where(q => q.Date >= request.Start && q.Date <= request.End);

            if (validEntries is null || !validEntries.Any())
            {
                return GenerateErrorResponse(HttpStatusCode.NotFound, ["No results."]);
            }

            var marketTimeZone = TimeZoneInfo.FindSystemTimeZoneById("CST");

            var availableFundsHold = request.PositionInfo.StartingBalance;
            var availableFundsHigh = request.PositionInfo.StartingBalance;

            var holdOpenPositions = new List<BackTestEntryResultCollection>();
            var highOpenPositions = new List<BackTestEntryResultCollection>();

            DateTimeOffset[] lastDate = [
                validEntries.Max(q => q.Results.Max(result => result.Hold.SoldAt)),
                validEntries.Max(q => q.Results.Max(result => result.High.SoldAt))
            ];

            var dayRange = Enumerable.Range(0, (lastDate.Max()- request.Start).Days + 1)
                .Select(day => request.Start.AddDays(day))
                .Where(day => day.DayOfWeek != DayOfWeek.Sunday && day.DayOfWeek != DayOfWeek.Saturday);

            var backtestDayResults = new List<BacktestDayV3>();

            foreach (var day in dayRange)
            {
                var offset = marketTimeZone.IsDaylightSavingTime(day.Date) ? TimeSpan.FromHours(-5) : TimeSpan.FromHours(-6);
                var marketOpen = new DateTimeOffset(day.Year, day.Month, day.Day, 8, 30, 0, offset);
                var marketClose = new DateTimeOffset(day.Year, day.Month, day.Day, 15, 0, 0, offset);

                var entry = validEntries.FirstOrDefault(q => q.Date == day.Date);

                var backtestEntryDay = new BacktestDayV3
                {
                    Date = day,
                    //CreditsUsed = results.First(result => result.Date == day.Date).CreditsUsed,
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
                    }
                };

                for (int i = 0; i < (marketClose - marketOpen).TotalMinutes; i++)
                {
                    var currentTime = marketOpen.AddMinutes(i);

                    var holdPositionsToSell = holdOpenPositions.Where(position => position.Hold.SoldAt == currentTime);
                    List<BackTestEntryResultCollection> holdPositionsToRemove = [];
                    foreach (var holdPosition in holdPositionsToSell)
                    {
                        availableFundsHold += holdPosition.Hold.EndPosition;
                        holdPositionsToRemove.Add(holdPosition);
                    }
                    foreach (var position in holdPositionsToRemove)
                    {
                        backtestEntryDay.Hold.Sold.Add(new BacktestDayPosition
                        {
                            Ticker = position.Ticker,
                            Price = position.Hold.EndPrice,
                            Shares = position.Shares,
                            Position = position.Hold.EndPosition,
                            Profit = position.Hold.Profit,
                            Timestamp = position.Hold.SoldAt,
                            StoppedOut = position.Hold.StoppedOut
                        });
                        holdOpenPositions.Remove(position);
                    }

                    var highPositionsToSell = highOpenPositions.Where(position => position.High.SoldAt == currentTime);
                    List<BackTestEntryResultCollection> highPositionsToRemove = [];
                    foreach (var highPosition in highPositionsToSell)
                    {
                        availableFundsHigh += highPosition.High.EndPosition;
                        highPositionsToRemove.Add(highPosition);
                    }
                    foreach (var position in highPositionsToRemove)
                    {
                        backtestEntryDay.High.Sold.Add(new BacktestDayPosition
                        {
                            Ticker = position.Ticker,
                            Price = position.High.EndPrice,
                            Shares = position.Shares,
                            Position = position.High.EndPosition,
                            Profit = position.High.Profit,
                            Timestamp = position.High.SoldAt,
                            StoppedOut = position.High.StoppedOut
                        });
                        highOpenPositions.Remove(position);
                    }

                    if (entry is null)
                    {
                        continue;
                    }

                    var holdResults = entry.Results.Where(result => result.BoughtAt == currentTime);

                    var feature = (request.Features is not null && request.Features.Any()) ? request.Features.FirstOrDefault(q => q.Type == FeatureType.TickerType) : null;

                    foreach (var holdResult in holdResults)
                    {
                        var tickerDetails = _marketCache.GetTickerDetails(holdResult.Ticker);

                        if (tickerDetails is null || (feature is not null && !feature.Value.Contains(tickerDetails.Type)))
                        {
                            continue;
                        }

                        if (availableFundsHold < request.PositionInfo.PositionSize || holdOpenPositions.Count >= request.PositionInfo.MaxConcurrentPositions)
                        {
                            continue;
                        }

                        backtestEntryDay.Hold.Bought.Add(new BacktestDayPosition
                        {
                            Ticker = holdResult.Ticker,
                            Price = holdResult.StartPrice,
                            Shares = holdResult.Shares,
                            Position = holdResult.StartPosition,
                            Timestamp = holdResult.BoughtAt,
                        });
                        holdOpenPositions.Add(holdResult);
                        availableFundsHold -= holdResult.StartPosition;
                    }

                    var highResults = entry.Results.Where(result => result.BoughtAt == currentTime);
                    foreach (var highResult in highResults)
                    {
                        if (availableFundsHigh < request.PositionInfo.PositionSize || highOpenPositions.Count >= request.PositionInfo.MaxConcurrentPositions)
                        {
                            continue;
                        }

                        backtestEntryDay.High.Bought.Add(new BacktestDayPosition
                        {
                            Ticker = highResult.Ticker,
                            Price = highResult.StartPrice,
                            Shares = highResult.Shares,
                            Position = highResult.StartPosition,
                            Timestamp = highResult.BoughtAt,
                        });
                        highOpenPositions.Add(highResult);
                        availableFundsHigh -= highResult.StartPosition;
                    }
                }

                backtestEntryDay.Hold.OpenPositions = holdOpenPositions.Count;
                backtestEntryDay.Hold.EndCashAvailable = availableFundsHold;
                backtestEntryDay.Hold.TotalBalance = holdOpenPositions.Sum(q => q.StartPosition) + backtestEntryDay.Hold.EndCashAvailable;

                backtestEntryDay.High.OpenPositions = highOpenPositions.Count;
                backtestEntryDay.High.EndCashAvailable = availableFundsHigh;
                backtestEntryDay.High.TotalBalance = highOpenPositions.Sum(q => q.StartPosition) + backtestEntryDay.High.EndCashAvailable;

                backtestDayResults.Add(backtestEntryDay);
            }

            var holdWins = backtestDayResults.SelectMany(q => q.Hold.Sold).Where(q => q.Profit > 0);
            var holdLosses = backtestDayResults.SelectMany(q => q.Hold.Sold).Where(q => q.Profit < 0);
            var highWins = backtestDayResults.SelectMany(q => q.High.Sold).Where(q => q.Profit > 0);
            var highLosses = backtestDayResults.SelectMany(q => q.High.Sold).Where(q => q.Profit < 0);

            var response = new OperationResult<BacktestV3Response>
            {
                Status = HttpStatusCode.OK,
                Data = new BacktestV3Response
                {
                    Id = request.Id,
                    HoldBalance = availableFundsHold,
                    HighBalance = availableFundsHigh,
                    MaxConcurrentPositions = backtestDayResults.Max(result => result.Hold.OpenPositions),
                    Hold = new BackTestEntryStatsV3
                    {
                        PositiveTrendRatio = (float)holdWins.Count() / (float)(holdWins.Count() + holdLosses.Count()),
                        AvgWin = holdWins.Average(q => q.Profit),
                        AvgLoss = holdLosses.Average(q => q.Profit),
                        AvgProfit = backtestDayResults.SelectMany(q => q.Hold.Sold).Average(q => q.Profit),
                        SumProfit = backtestDayResults.SelectMany(q => q.Hold.Sold).Sum(q => q.Profit),
                    },
                    High = new BackTestEntryStatsV3
                    {
                        PositiveTrendRatio = (float)highWins.Count() / (float)(highWins.Count() + highLosses.Count()),
                        AvgWin = highWins.Average(q => q.Profit),
                        AvgLoss = highLosses.Average(q => q.Profit),
                        AvgProfit = backtestDayResults.SelectMany(q => q.High.Sold).Average(q => q.Profit),
                        SumProfit = backtestDayResults.SelectMany(q => q.High.Sold).Sum(q => q.Profit)
                    },
                    Results = backtestDayResults
                }
            };

            var newRecord = new BacktestRecord
            {
                Id = request.Id,
                CustomerId = Guid.Empty.ToString(),
                Date = DateTimeOffset.Now.ToString("yyyy-MM-dd hh:mm z"),
                CreditsUsed = validEntries.Where(result => result is not null).Sum(result => result.CreditsUsed),
                HoldProfit = response.Data.Hold.SumProfit,
                HighProfit = response.Data.High.SumProfit,
                RequestDetails = $"{JsonSerializer.Serialize(request.Exit)}{JsonSerializer.Serialize(request.Argument)}",
                StartDate = int.Parse(request.Start.ToString("yyyyMMdd")),
                EndDate = int.Parse(request.End.ToString("yyyyMMdd")),
                S3ObjectName = record is null ? Guid.NewGuid().ToString() : record.S3ObjectName
            };

            await _dbContext.SaveAsync(newRecord, cancellationToken);

            if (record is null)
            {
                var s3Response = await _s3Client.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = "lad-dev-marketviewer",
                    Key = $"backtestResults/{newRecord.S3ObjectName}",
                    ContentBody = JsonSerializer.Serialize(validEntries)
                }, cancellationToken);
            }

            response.Data.Other = validEntries;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: {}", ex.Message);
            _logger.LogError("Stacktrace: {}", ex.StackTrace);

            return new OperationResult<BacktestV3Response>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = [ex.Message]
            };
        }
    }

    private static OperationResult<BacktestV3Response> GenerateErrorResponse(HttpStatusCode status, IEnumerable<string> errors)
    {
        return new OperationResult<BacktestV3Response>
        {
            Status = status,
            ErrorMessages = errors
        };
    }
}
