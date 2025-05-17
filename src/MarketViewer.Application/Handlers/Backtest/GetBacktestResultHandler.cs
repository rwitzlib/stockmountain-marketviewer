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
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MarketViewer.Application.Handlers.Backtest;

public class GetBacktestResultHandler(
    IAmazonLambda _lambdaClient,
    IAmazonDynamoDB _dynamoDbClient,
    ILogger<GetBacktestResultHandler> _logger) : IRequestHandler<GetBacktestResultRequest, OperationResult<GetBacktestResultResponse>>
{
    public async Task<OperationResult<GetBacktestResultResponse>> Handle(GetBacktestResultRequest request, CancellationToken cancellationToken)
    {
        if (request is null || request.Id is null)
        {
            return new OperationResult<GetBacktestResultResponse>
            {
                Status = HttpStatusCode.BadRequest,
                ErrorMessages = ["Invalid request."]
            };
        }

        var json = JsonSerializer.Serialize(request);

        var lambdaResponse = await _lambdaClient.InvokeAsync(new InvokeRequest
        {
            FunctionName = "lad-dev-backtest-orchestrator",
            Payload = json
        }, cancellationToken);

        if (lambdaResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("Unable to start backtest.");
            return new OperationResult<GetBacktestResultResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal server error."]
            };
        }

        var item = Document.FromJson(JsonSerializer.Serialize(new BacktestRecord
        {
            Id = request.Id,
            CustomerId = request.UserId,
            Status = BacktestStatus.Pending,
            CreatedAt = DateTimeOffset.Now.ToString()
        }));

        var dynamodbResponse = await _dynamoDbClient.PutItemAsync(new PutItemRequest
        {
            TableName = "lad-dev-marketviewer-backtest-store",
            Item = item.ToAttributeMap()
        }, cancellationToken);

        if (dynamodbResponse.HttpStatusCode != HttpStatusCode.OK)
        {
            _logger.LogError("Unable to store backtest record.");
            return new OperationResult<GetBacktestResultResponse>
            {
                Status = HttpStatusCode.InternalServerError,
                ErrorMessages = ["Internal server error."]
            };
        }

        return new OperationResult<GetBacktestResultResponse>
        {
            Status = HttpStatusCode.OK,
            Data = new GetBacktestResultResponse
            {
                Id = request.Id,
                Status = BacktestStatus.Pending
            }
        };
    }
}
