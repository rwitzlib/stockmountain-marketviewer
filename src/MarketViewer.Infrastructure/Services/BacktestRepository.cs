using Amazon.Lambda.Model;
using Amazon.Lambda;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Document = Amazon.DynamoDBv2.DocumentModel.Document;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Responses.Market.Backtest;
using MarketViewer.Contracts.Requests.Market.Backtest;
using MarketViewer.Core.Services;
using System.Threading;
using MarketViewer.Infrastructure.Config;

namespace MarketViewer.Infrastructure.Services;

public class BacktestRepository(
    BacktestConfig config,
    IAmazonDynamoDB dynamoDb,
    IAmazonS3 s3,
    IAmazonLambda lambda,
    ILogger<BacktestRepository> logger) : IBacktestRepository
{
    public async Task<bool> Create(BacktestRecord record, IEnumerable<BacktestLambdaResponseV3> entries)
    {
        try
        {
            var putRequest = new PutItemRequest
            {
                TableName = config.TableName,
                Item = Document.FromJson(JsonSerializer.Serialize(record)).ToAttributeMap()
            };
            var response = await dynamoDb.PutItemAsync(putRequest);

            if (entries is null)
            {
                var s3Response = await s3.PutObjectAsync(new PutObjectRequest
                {
                    BucketName = config.S3BucketName,
                    Key = $"backtestResults/{record.S3ObjectName}",
                    ContentBody = JsonSerializer.Serialize(entries)
                });
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating backtest record");
            return false;
        }
    }

    public bool CheckForBacktestHistory(string compressedRequest, out BacktestRecord record)
    {
        record = null;
        
        try
        {
            if (string.IsNullOrWhiteSpace(compressedRequest))
            {
                return false;
            }

            var dynamoDbRespnonse = dynamoDb.QueryAsync(new QueryRequest
            {
                TableName = config.TableName,
                IndexName = config.RequestIndexName,
                KeyConditionExpression = "RequestDetails = :request",
                ExpressionAttributeValues =
                {
                    {
                        ":request",
                        new AttributeValue
                        {
                            S = compressedRequest
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

    public async Task<List<BacktestLambdaResponseV3>> GetBacktestResultsFromLambda(BacktestRequestV3 request)
    {
        var days = request.End == request.Start ? [request.Start] : Enumerable.Range(0, (request.End - request.Start).Days + 1)
            .Select(day => request.Start.AddDays(day))
            .Where(day => day.DayOfWeek != DayOfWeek.Sunday && day.DayOfWeek != DayOfWeek.Saturday);

        logger.LogInformation("Backtesting strategy between {start} and {end}. Total days: {count}",
            request.Start.ToString("yyyy-MM-dd"),
            request.End.ToString("yyyy-MM-dd"),
            days.Count());

        var tasks = new List<Task<BacktestLambdaResponseV3>>();
        foreach (var day in days)
        {
            var backtesterLambdaRequest = new BacktestLambdaRequestV3
            {
                Date = day.Date,
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

    public async Task<List<BacktestLambdaResponseV3>> GetBacktestResultsFromS3(BacktestRecord record)
    {
        try
        {
            if (record is null || record.S3ObjectName is null)
            {
                return [];
            }

            var s3Response = await s3.GetObjectAsync(new GetObjectRequest
            {
                BucketName = config.S3BucketName,
                Key = $"backtestResults/{record.S3ObjectName}"
            });

            using var streamReader = new StreamReader(s3Response.ResponseStream);
            var json = await streamReader.ReadToEndAsync();

            var s3Results = JsonSerializer.Deserialize<IEnumerable<BacktestLambdaResponseV3>>(json);
            s3Results.ToList().ForEach(q => q.CreditsUsed = 0);

            return s3Results.ToList();
        }
        catch (Exception e)
        {
            return [];
        }
    }

    private async Task<BacktestLambdaResponseV3> BacktestDay(BacktestLambdaRequestV3 request)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);

            var invokeRequest = new InvokeRequest
            {
                FunctionName = config.LambdaName,
                InvocationType = InvocationType.RequestResponse,
                Payload = json,

            };

            var response = await lambda.InvokeAsync(invokeRequest);

            if (response.StatusCode is not 200)
            {
                return null;
            }

            var streamReader = new StreamReader(response.Payload);
            var result = streamReader.ReadToEnd();

            var backtestEntry = JsonSerializer.Deserialize<BacktestLambdaResponseV3>(result);

            return backtestEntry;
        }
        catch (Exception e)
        {
            return null;
        }
    }
}
