using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Enums.Strategy;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Core.Services;
using MarketViewer.Infrastructure.Config;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace MarketViewer.Infrastructure.Services;

public class StrategyRepository(StrategyConfig config, IAmazonDynamoDB dynamoDb, ILogger<StrategyRepository> logger) : IStrategyRepository
{
    public async Task<StrategyDto> Put(StrategyDto strategy)
    {
        try
        {
            var request = new PutItemRequest
            {
                TableName = config.TableName,
                Item = MapToDynamoDbItem(strategy)
            };

            await dynamoDb.PutItemAsync(request);
            return strategy;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error putting strategy with ID {Id}", strategy.Id);
            return null;
        }
    }

    public async Task<StrategyDto> Get(string id)
    {
        try
        {
            var request = new GetItemRequest
            {
                TableName = config.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = id } }
                }
            };

            var response = await dynamoDb.GetItemAsync(request);
            return response.Item == null ? null : MapToStrategyDto(response.Item);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting strategy with ID {Id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<StrategyDto>> ListByUser(string userId)
    {
        try
        {
            var request = new QueryRequest
            {
                TableName = config.TableName,
                IndexName = config.UserIndexName,
                KeyConditionExpression = "UserId = :userId",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":userId", new AttributeValue { S = userId } }
                }
            };

            var response = await dynamoDb.QueryAsync(request);
            return response.Items.Select(MapToStrategyDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all strategies");
            return [];
        }
    }

    public async Task<IEnumerable<StrategyDto>> ListByPublic(bool isPublic)
    {
        try
        {
            var request = new QueryRequest
            {
                TableName = config.TableName,
                IndexName = config.PublicIndexName,
                KeyConditionExpression = "Public = :isPublic",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":isPublic", new AttributeValue { S = isPublic.ToString() } }
                }
            };

            var response = await dynamoDb.QueryAsync(request);
            return response.Items.Select(MapToStrategyDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting public strategies");
            return [];
        }
    }

    public async Task<bool> Delete(string id)
    {
        try
        {
            var request = new DeleteItemRequest
            {
                TableName = config.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = id } }
                }
            };

            var response = await dynamoDb.DeleteItemAsync(request);

            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                logger.LogError("Failed to delete strategy with ID {Id}", id);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting strategy with ID {Id}", id);
            throw; // Re-throw for delete operations since we can't return null
        }
    }

    #region Private Methods

    private static StrategyDto MapToStrategyDto(Dictionary<string, AttributeValue> item)
    {
        return new StrategyDto
        {
            Id = item["Id"].S,
            UserId = item["UserId"].S,
            Name = item["Name"].S,
            Enabled = item["Enabled"].BOOL ?? false,
            Type = Enum.Parse<TradeType>(item["Type"].S),
            Public = bool.Parse(item["Public"].S),
            Integration = Enum.Parse<IntegrationType>(item["Integration"].S),
            PositionInfo = JsonSerializer.Deserialize<BacktestPositionInformation>(item["PositionInfo"].S),
            ExitInfo = JsonSerializer.Deserialize<BacktestExitInformation>(item["ExitInfo"].S),
            Argument = JsonSerializer.Deserialize<ScanArgument>(item["Argument"].S)
        };
    }

    private static Dictionary<string, AttributeValue> MapToDynamoDbItem(StrategyDto strategy)
    {
        return new Dictionary<string, AttributeValue>
        {
            { "Id", new AttributeValue { S = strategy.Id } },
            { "UserId", new AttributeValue { S = strategy.UserId } },
            { "Name", new AttributeValue { S = strategy.Name } },
            { "Enabled", new AttributeValue { BOOL = strategy.Enabled } },
            { "Type", new AttributeValue { S = strategy.Type.ToString() } },
            { "Public", new AttributeValue { S = strategy.Public.ToString() } },
            { "Integration", new AttributeValue { S = strategy.Integration.ToString() } },
            { "PositionInfo", new AttributeValue { S = JsonSerializer.Serialize(strategy.PositionInfo) } },
            { "ExitInfo", new AttributeValue { S = JsonSerializer.Serialize(strategy.ExitInfo) } },
            { "Argument", new AttributeValue { S = JsonSerializer.Serialize(strategy.Argument) } }
        };
    }

    #endregion
}
