using System.Text.Json;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Document = Amazon.DynamoDBv2.DocumentModel.Document;
using MarketViewer.Contracts.Responses.Market.Backtest;
using MarketViewer.Core.Services;
using MarketViewer.Infrastructure.Config;
using MarketViewer.Contracts.Records;

namespace MarketViewer.Infrastructure.Services;

public class BacktestRepository(
    BacktestConfig config,
    IAmazonDynamoDB dynamoDb,
    IAmazonS3 s3,
    ILogger<BacktestRepository> logger) : IBacktestRepository
{
    public async Task<bool> Put(BacktestRecord record, IEnumerable<WorkerResponse> entries = null)
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

    public async Task<BacktestRecord> Get(string id)
    {
        try
        {
            var response = await dynamoDb.GetItemAsync(new GetItemRequest
            {
                TableName = config.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = id } }
                }
            });

            if (response.Item == null || response.Item.Count == 0)
            {
                return null;
            }

            var json = Document.FromAttributeMap(response.Item).ToJson();
            return JsonSerializer.Deserialize<BacktestRecord>(json);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving backtest record with ID {id}", id);
            return null;
        }
    }

    public async Task<List<BacktestRecord>> List(string userId)
    {
        try
        {
            var response = await dynamoDb.QueryAsync(new QueryRequest
            {
                TableName = config.TableName,
                IndexName = config.UserIndexName,
                KeyConditionExpression = "UserId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = userId } }
                }
            });

            if (response == null || response.Count <= 0)
            {
                return null;
            }

            return response.Items
                .Select(item => Document.FromAttributeMap(item).ToJson())
                .Select(json => JsonSerializer.Deserialize<BacktestRecord>(json))
                .ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing backtest records for user {userId}", userId);
            return null;
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
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
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
            logger.LogError(e, "Error checking for backtest history");
            return false;
        }
    }

    public async Task<List<WorkerResponse>> GetBacktestResultsFromS3(BacktestRecord record)
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

            var s3Results = JsonSerializer.Deserialize<IEnumerable<WorkerResponse>>(json);
            s3Results.ToList().ForEach(q => q.CreditsUsed = 0);

            return s3Results.ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error retrieving backtest results from S3 for record {recordId}", record.Id);
            return [];
        }
    }
}
