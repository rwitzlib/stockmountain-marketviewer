using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using MarketViewer.Contracts.Enums.Backtest;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Records;
using MarketViewer.Contracts.Requests.Market.Backtest;
using MarketViewer.Contracts.Responses.Market.Backtest;
using MarketViewer.Infrastructure.Config;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MarketViewer.Application.Handlers.Market.Backtest;

public class StartBacktestHandler(
    IAmazonLambda lambda,
    IDynamoDBContext dynamodb,
    ServiceConfigs configuration,
    ILogger<StartBacktestHandler> logger) : IRequestHandler<StartBacktestRequest, OperationResult<StartBacktestResponse>>
{
    public async Task<OperationResult<StartBacktestResponse>> Handle(StartBacktestRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request is null || string.IsNullOrWhiteSpace(request.Id))
            {
                return new OperationResult<StartBacktestResponse>
                {
                    Status = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Invalid request."]
                };
            }

            logger.LogInformation("Starting backtest with Id: {BacktestId}", request.Id);

            var record = new BacktestRecord
            {
                Id = request.Id,
                CustomerId = request.UserId,
                Status = BacktestStatus.Pending,
                CreatedAt = DateTimeOffset.Now.ToString()
            };

            await dynamodb.SaveAsync(record, cancellationToken);

            var json = JsonSerializer.Serialize(request);

            var lambdaResponse = lambda.InvokeAsync(new InvokeRequest
            {
                FunctionName = configuration.BacktestOrchestratorLambdaName,
                Payload = json
            }, cancellationToken);

            return new OperationResult<StartBacktestResponse>
            {
                Status = HttpStatusCode.OK,
                Data = new StartBacktestResponse
                {
                    Id = request.Id,
                    Status = BacktestStatus.Pending
                }
            };
        }
        catch (Exception e)
        {
            logger.LogError("Exception: {message}", e.Message);
            return new OperationResult<StartBacktestResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal error."]
            };
        }
    }
}
