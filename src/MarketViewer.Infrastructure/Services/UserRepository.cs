using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using MarketViewer.Core.Enums;
using MarketViewer.Core.Records;
using MarketViewer.Core.Services;
using MarketViewer.Infrastructure.Config;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MarketViewer.Infrastructure.Services;

public class UserRepository(UserConfig config, IAmazonDynamoDB dynamodb, ILogger<UserRepository> logger) : IUserRepository
{
    public async Task<bool> Put(UserRecord record)
    {
        try
        {
            var item = new Dictionary<string, AttributeValue>
            {
                { "Id", new AttributeValue { S = record.Id } },
                { "Role", new AttributeValue { S = record.Role.ToString() } },
                { "AvatarUrl", new AttributeValue { S = record.AvatarUrl } },
                { "IsPublic", new AttributeValue { S = record.IsPublic.ToString() } },
                { "Credits", new AttributeValue { N = record.Credits.ToString() } }
            };
            var putItemRequest = new PutItemRequest
            {
                TableName = config.TableName,
                Item = item
            };
            var response = await dynamodb.PutItemAsync(putItemRequest);
            return response.HttpStatusCode == HttpStatusCode.OK;

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error putting user record: {message}", ex.Message);
            return false;
        }
    }

    public async Task<UserRecord> Get(string id)
    {
        try
        {
            var queryResponse = await dynamodb.GetItemAsync(new GetItemRequest
            {
                TableName = config.TableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = id } }
                }
            });

            if (queryResponse.HttpStatusCode != HttpStatusCode.OK || queryResponse.Item == null || !queryResponse.IsItemSet)
            {
                return null;
            }

            var userRecord = new UserRecord
            {
                Id = queryResponse.Item["Id"].S,
                Role = Enum.Parse<UserRole>(queryResponse.Item["Role"].S),
                AvatarUrl = queryResponse.Item["AvatarUrl"].S,
                IsPublic = bool.Parse(queryResponse.Item["IsPublic"].S),
                Credits = float.Parse(queryResponse.Item["Credits"].N),
            };

            return userRecord;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting user record: {message}", e.Message);
            return null;
        }
    }
}
