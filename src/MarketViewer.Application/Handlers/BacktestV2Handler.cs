using Amazon.Lambda;
using Amazon.Lambda.Model;
using FluentValidation;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;
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
    IAmazonLambda amazonLambda,
    ILogger<BacktestV2Handler> logger) : IRequestHandler<BacktestV2Request, OperationResult<BacktestResponse>>
{
    public async Task<OperationResult<BacktestResponse>> Handle(BacktestV2Request request, CancellationToken cancellationToken)
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

        logger.LogInformation("Backtesting strategy between {start} and {end}. Total days: {count}",
            request.Start.ToString("yyyy-MM-dd"),
            request.End.ToString("yyyy-MM-dd"),
            days.Count());

        var tasks = new List<Task<BacktestEntry>>();
        foreach (var day in days)
        {
            var backtesterLambdaRequest = new BacktesterLambdaV2Request
            {
                Timestamp = day.Date,
                ExitType = request.ExitType,
                ExitStrategy = request.ExitStrategy,
                PositionSize = request.PositionSize,
                Multiplier = request.Multiplier,
                Timespan = request.Timespan,
                Argument = request.Argument,
                Features = request.Features,
            };
            tasks.Add(Task.Run(async () => await BacktestDay(backtesterLambdaRequest)));
        }
        var results = await Task.WhenAll(tasks);
        var validResults = results.Where(q => q is not null);

        if (validResults is null || !validResults.Any())
        {
            return GenerateErrorResponse(HttpStatusCode.NotFound, ["No results."]);
        }

        var longAvg = Math.Truncate(validResults.Sum(entry => entry.LongRatio) / validResults.Count() * 100) / 100;
        var shortAvg = Math.Truncate(validResults.Sum(entry => entry.ShortRatio) / validResults.Count() * 100) / 100;

        var longPositionAvgChange = validResults.Sum(entry => entry.LongPositionChange) / validResults.Count();
        var shortPositionAvgChange = validResults.Sum(entry => entry.ShortPositionChange) / validResults.Count();

        return new OperationResult<BacktestResponse>
        {
            Status = HttpStatusCode.OK,
            Data = new BacktestResponse
            {
                RequestId = Guid.NewGuid(),
                Results = validResults,
                ResultsCount = validResults.Count(),
                LongRatioAvg = longAvg,
                ShortRatioAvg = shortAvg,
                LongPositionAvgChange = longPositionAvgChange,
                ShortPositionAvgChange = shortPositionAvgChange
            }
        };
    }

    private async Task<BacktestEntry> BacktestDay(BacktesterLambdaV2Request request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);

            var invokeRequest = new InvokeRequest
            {
                FunctionName = "lad-dev-backtester-v2",
                InvocationType = InvocationType.RequestResponse,
                Payload = json
            };

            var response = await amazonLambda.InvokeAsync(invokeRequest);

            if (response.StatusCode is not 200)
            {
                return null;
            }

            var streamReader = new StreamReader(response.Payload);
            var result = streamReader.ReadToEnd();

            var backtestEntry = JsonSerializer.Deserialize<BacktestEntry>(result);

            return backtestEntry;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    private static OperationResult<BacktestResponse> GenerateErrorResponse(HttpStatusCode status, IEnumerable<string> errors)
    {
        return new OperationResult<BacktestResponse>
        {
            Status = status,
            ErrorMessages = errors
        };
    }
}
