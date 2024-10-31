using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Model;
using Amazon.Lambda;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Requests.Backtest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Documents;
using Document = Amazon.DynamoDBv2.DocumentModel.Document;

namespace MarketViewer.Infrastructure.Services;

public class BacktestService(
    IDynamoDBContext _dbContext,
    IAmazonDynamoDB _dynamodbClient,
    IAmazonS3 _s3Client,
    IAmazonLambda _lambdaClient,
    ILogger<BacktestService> _logger)
{
    public bool CheckForBacktestHistory(BacktestV3Request request, out BacktestRecord record)
    {
        record = null;
        
        try
        {
            if (request is null || request.Exit is null || request.Argument is null)
            {
                return false;
            }

            var dynamoDbRespnonse = _dynamodbClient.QueryAsync(new QueryRequest
            {
                TableName = "lad-dev-marketviewer-backtest-store",
                IndexName = "RequestIndex",
                KeyConditionExpression = "RequestDetails = :request",
                FilterExpression = "StartDate <= :start AND EndDate >= :end",
                ExpressionAttributeValues =
                {
                    {
                        ":request",
                        new AttributeValue
                        {
                            S = $"{JsonSerializer.Serialize(request.Exit)}{JsonSerializer.Serialize(request.Argument)}"
                        }
                    },
                    {
                        ":start",
                        new AttributeValue
                        {
                            N = $"{request.Start:yyyyMMdd}"
                        }
                    },
                    {
                        ":end",
                        new AttributeValue
                        {
                            N = $"{request.End:yyyyMMdd}"
                        }
                    }
                }
            }).Result;

            if (dynamoDbRespnonse == null || dynamoDbRespnonse.Count <= 0)
            {
                return false;
            }

            var json = Document.FromAttributeMap(dynamoDbRespnonse.Items.FirstOrDefault()).ToJson();
            record = JsonSerializer.Deserialize<BacktestRecord>(json);

            return record != null;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    public async Task<List<BacktestEntryV3>> GetBacktestResultsFromLambda(BacktestV3Request request)
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
        var taskResults = await Task.WhenAll(tasks);
        var lambdaResults = taskResults.Where(q => q is not null && q.Results is not null);

        return lambdaResults.ToList();
    }

    public async Task<List<BacktestEntryV3>> GetBacktestResultsFromS3(BacktestRecord record)
    {
        try
        {
            if (record is null || record.S3ObjectName is null)
            {
                return [];
            }

            var s3Response = await _s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = "lad-dev-marketviewer",
                Key = $"backtestResults/{record.S3ObjectName}"
            });

            using var streamReader = new StreamReader(s3Response.ResponseStream);
            var json = await streamReader.ReadToEndAsync();

            var s3Results = JsonSerializer.Deserialize<IEnumerable<BacktestEntryV3>>(json);
            s3Results.ToList().ForEach(q => q.CreditsUsed = 0);

            return s3Results.ToList();
        }
        catch (Exception e)
        {
            return [];
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
}
