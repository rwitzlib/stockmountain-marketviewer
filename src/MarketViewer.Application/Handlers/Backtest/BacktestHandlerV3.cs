using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using FluentValidation;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Requests.Backtest;
using MarketViewer.Contracts.Responses.Backtest;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MarketViewer.Application.Handlers.Backtest;

public class BacktestHandlerV3(
    IAmazonLambda _lambdaClient,
    IDynamoDBContext _dbContext,
    ILogger<BacktestHandlerV3> _logger) : IRequestHandler<BacktestV3Request, OperationResult<BacktestV3Response>>
{
    public async Task<OperationResult<BacktestV3Response>> Handle(BacktestV3Request request, CancellationToken cancellationToken)
    {
        try
        {
            var days = request.End == request.Start ? [request.Start] : Enumerable.Range(0, (request.End - request.Start).Days + 1)
                .Select(day => request.Start.AddDays(day))
                .Where(day => day.DayOfWeek != DayOfWeek.Sunday && day.DayOfWeek != DayOfWeek.Saturday);

            _logger.LogInformation("Backtesting strategy between {start} and {end}. Total days: {count}",
                request.Start.ToString("yyyy-MM-dd"),
                request.End.ToString("yyyy-MM-dd"),
                days.Count());

            var tasks = new List<Task<BacktestEntryV3>>();
            foreach (var day in days)
            {
                var backtesterLambdaRequest = new BacktesterLambdaV3Request
                {
                    Date = day.Date,
                    DetailedResponse = request.DetailedResponse,
                    PositionInfo = request.PositionInfo,
                    Exit = request.Exit,
                    Features = request.Features,
                    Argument = request.Argument,
                };
                tasks.Add(Task.Run(async () => await BacktestDay(backtesterLambdaRequest)));
            }
            var results = await Task.WhenAll(tasks);
            var validResults = results.Where(q => q is not null && q.Results is not null);

            var marketTimeZone = TimeZoneInfo.FindSystemTimeZoneById("CST");

            var availableFundsHold = request.PositionInfo.StartingBalance;
            var availableFundsHigh = request.PositionInfo.StartingBalance;

            var holdOpenPositions = new List<BackTestEntryResultCollection>();
            var highOpenPositions = new List<BackTestEntryResultCollection>();

            DateTimeOffset[] lastDate = [
                validResults.Max(q => q.Results.Max(result => result.Hold.SoldAt)),
                validResults.Max(q => q.Results.Max(result => result.High.SoldAt))
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

                var entry = validResults.FirstOrDefault(q => q.Date == day.Date);

                var backtestEntryDay = new BacktestDayV3
                {
                    Date = day,
                    //CreditsUsed = results.First(result => result.Date == day.Date).CreditsUsed,
                    Hold = new BacktestDayDetails
                    {
                        StartingBalance = availableFundsHold,
                        Bought = [],
                        Sold = []
                    },
                    High = new BacktestDayDetails
                    {
                        StartingBalance = availableFundsHigh,
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
                    foreach (var holdResult in holdResults)
                    {
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
                backtestEntryDay.Hold.EndingBalance = availableFundsHold;
                backtestEntryDay.High.OpenPositions = highOpenPositions.Count;
                backtestEntryDay.High.EndingBalance = availableFundsHigh;

                //backtestEntryDay.Results = entry is not null ? entry.Results : [];

                backtestDayResults.Add(backtestEntryDay);
            }

            if (validResults is null || !validResults.Any())
            {
                return GenerateErrorResponse(HttpStatusCode.NotFound, ["No results."]);
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
                    StartBalance = request.PositionInfo.StartingBalance,
                    HoldBalance = availableFundsHold,
                    HighBalance = availableFundsHigh,
                    PotentialHoldProfit = validResults.Sum(q => q.Hold.SumProfit),
                    PotentialHighProfit = validResults.Sum(q => q.High.SumProfit),
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

            var record = new BacktestRecord
            {
                Id = request.Id,
                CustomerId = Guid.Empty.ToString(),
                Date = DateTimeOffset.Now.ToString("yyyy-MM-dd hh:mm z"),
                CreditsUsed = results.Where(result => result is not null).Sum(result => result.CreditsUsed),
                HoldProfit = response.Data.Hold.SumProfit,
                HighProfit = response.Data.High.SumProfit,
                Request = JsonSerializer.Serialize(request),
                Response = JsonSerializer.Serialize(response.Data)
            };

            await _dbContext.SaveAsync(record, cancellationToken);

            response.Data.Other = validResults;

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

    private async Task<BacktestEntryV3> BacktestDay(BacktesterLambdaV3Request request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);

            var invokeRequest = new InvokeRequest
            {
                FunctionName = "lad-dev-backtester-v3",
                InvocationType = InvocationType.RequestResponse,
                Payload = json,

            };

            var response = await _lambdaClient.InvokeAsync(invokeRequest);

            if (response.StatusCode is not 200)
            {
                return null;
            }

            var streamReader = new StreamReader(response.Payload);
            var result = streamReader.ReadToEnd();

            var backtestEntry = JsonSerializer.Deserialize<BacktestEntryV3>(result);

            return backtestEntry;
        }
        catch (Exception e)
        {
            return null;
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
