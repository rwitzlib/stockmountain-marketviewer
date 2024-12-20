using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using MarketViewer.Contracts.Enums.Backtest;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Requests.Backtest;
using MarketViewer.Contracts.Responses.Backtest;
using MarketViewer.Core.Config;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MarketViewer.Application.Handlers.Backtest;

public class StartBacktestHandler(
    IAmazonLambda _lambdaClient,
    IAmazonDynamoDB _dynamoDbClient,
    ServiceConfigs configuration,
    ILogger<StartBacktestHandler> _logger) : IRequestHandler<StartBacktestRequest, OperationResult<StartBacktestResponse>>
{
    public async Task<OperationResult<StartBacktestResponse>> Handle(StartBacktestRequest request, CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Id))
        {
            return new OperationResult<StartBacktestResponse>
            {
                Status = HttpStatusCode.BadRequest,
                ErrorMessages = ["Invalid request."]

            };
        }

        _logger.LogInformation("Starting backtest with ID: {BacktestId}", request.Id);

        var item = Document.FromJson(JsonSerializer.Serialize(new BacktestRecord
        {
            Id = request.Id,
            CustomerId = request.UserId,
            Status = BacktestStatus.Pending,
            CreatedAt = DateTimeOffset.Now.ToString()
        }));

        var dynamodbResponse = await _dynamoDbClient.PutItemAsync(new PutItemRequest
        {
            TableName = configuration.BacktestStore,
            Item = item.ToAttributeMap()
        }, cancellationToken);

        if (dynamodbResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("Unable to store backtest record.");
            return new OperationResult<StartBacktestResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal server error."]
            };
        }

        var json = JsonSerializer.Serialize(request);

        var lambdaResponse = await _lambdaClient.InvokeAsync(new InvokeRequest
        {
            FunctionName = configuration.BacktestOrchestrator,
            Payload = json
        }, cancellationToken);

        if (lambdaResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("Unable to start backtest.");
            return new OperationResult<StartBacktestResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal server error."]
            };
        }

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
}
