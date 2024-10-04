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

namespace MarketViewer.Application.Handlers;

public class BacktestV2Handler(
    //IValidator<BacktestV2Request> validator,
    IAmazonLambda _lambdaClient,
    IDynamoDBContext _dbContext,
    ILogger<BacktestV2Handler> _logger) : IRequestHandler<BacktestV2Request, OperationResult<BacktestV2Response>>
{
    public async Task<OperationResult<BacktestV2Response>> Handle(BacktestV2Request request, CancellationToken cancellationToken)
    {
        try
        {
            //var validationResult = validator.Validate(request);

            //if (!validationResult.IsValid)
            //{
            //    var errorMessages = validationResult.Errors.Select(error => error.ErrorMessage);
            //    return GenerateErrorResponse(HttpStatusCode.BadRequest, errorMessages);
            //}

            var days = (request.End == request.Start) ? [request.Start] : Enumerable.Range(0, (request.End - request.Start).Days + 1)
                .Select(day => request.Start.AddDays(day))
                .Where(day => day.DayOfWeek != DayOfWeek.Sunday && day.DayOfWeek != DayOfWeek.Saturday);

            _logger.LogInformation("Backtesting strategy between {start} and {end}. Total days: {count}",
                request.Start.ToString("yyyy-MM-dd"),
                request.End.ToString("yyyy-MM-dd"),
                days.Count());


            var tasks = new List<Task<BacktestEntryV2>>();
            foreach (var day in days)
            {
                var backtesterLambdaRequest = new BacktesterLambdaV2Request
                {
                    Timestamp = day.Date,
                    DetailedResponse = request.DetailedResponse,
                    PositionSize = request.PositionSize,
                    Multiplier = request.Multiplier,
                    StopLoss = request.StopLoss,
                    MaxPositions = request.MaxPositions,
                    Timespan = request.Timespan,
                    Argument = request.Argument,
                    Features = request.Features,
                };
                tasks.Add(Task.Run(async () => await BacktestDay(backtesterLambdaRequest)));
            }
            var results = await Task.WhenAll(tasks);
            var validResults = results.Where(q => q is not null && q.Results is not null);

            if (validResults is null || !validResults.Any())
            {
                return GenerateErrorResponse(HttpStatusCode.NotFound, ["No results."]);
            }

            var response = new OperationResult<BacktestV2Response>
            {
                Status = HttpStatusCode.OK,
                Data = new BacktestV2Response
                {
                    Id = request.Id,
                    Hold = new BackTestEntryStatsV2
                    {
                        PositiveTrendRatio = validResults.Average(result => result.Hold.PositiveTrendRatio),
                        HighPosition = validResults.Average(result => result.Hold.HighPosition),
                        LowPosition = validResults.Average(result => result.Hold.LowPosition),
                        AvgProfit = validResults.Average(result => result.Hold.SumProfit),
                        SumProfit = validResults.Sum(result => result.Hold.SumProfit),
                    },
                    High = new BackTestEntryStatsV2
                    {
                        PositiveTrendRatio = validResults.Average(result => result.High.PositiveTrendRatio),
                        HighPosition = validResults.Average(result => result.High.HighPosition),
                        LowPosition = validResults.Average(result => result.High.LowPosition),
                        AvgProfit = validResults.Average(result => result.High.SumProfit),
                        SumProfit = validResults.Sum(result => result.High.SumProfit),
                    },
                    Other = new BackTestEntryStatsV2
                    {
                        PositiveTrendRatio = validResults.Average(result => result.Other.PositiveTrendRatio),
                        HighPosition = validResults.Average(result => result.Other.HighPosition),
                        LowPosition = validResults.Average(result => result.Other.LowPosition),
                        AvgProfit = validResults.Average(result => result.Other.SumProfit),
                        SumProfit = validResults.Sum(result => result.Other.SumProfit),
                    }
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

            response.Data.Results = validResults;

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error: {}", ex.Message);
            _logger.LogError("Stacktrace: {}", ex.StackTrace);

            return new OperationResult<BacktestV2Response>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = [ex.Message]
            };
        }
    }

    private async Task<BacktestEntryV2> BacktestDay(BacktesterLambdaV2Request request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);

            var invokeRequest = new InvokeRequest
            {
                FunctionName = "lad-dev-backtester-v2",
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

            var backtestEntry = JsonSerializer.Deserialize<BacktestEntryV2>(result);

            return backtestEntry;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    private static OperationResult<BacktestV2Response> GenerateErrorResponse(HttpStatusCode status, IEnumerable<string> errors)
    {
        return new OperationResult<BacktestV2Response>
        {
            Status = status,
            ErrorMessages = errors
        };
    }
}
