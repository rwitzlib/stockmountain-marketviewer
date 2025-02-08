using Amazon.DynamoDBv2.DataModel;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Core.Models;

[ExcludeFromCodeCoverage]
[DynamoDBTable("lad-dev-marketviewer-user-store")]
public class User
{
    [DynamoDBHashKey]
    public string Id { get; set; }
    public float Credits { get; set; }
}
