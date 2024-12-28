using Amazon.DynamoDBv2.DataModel;
using MarketViewer.Contracts.Enums.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Backtest;

/// <summary>
/// Database record for storing information about backtest results.
/// </summary>
[ExcludeFromCodeCoverage]
[DynamoDBTable("lad-dev-marketviewer-backtest-store")]
public class BacktestRecord
{
    [DynamoDBHashKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [DynamoDBGlobalSecondaryIndexHashKey]
    public string CustomerId { get; set; }
    public BacktestStatus Status { get; set; }
    public string CreatedAt { get; set; }

    /// <summary>
    /// Cost is $0.000133334 per credit assuming 2048 MB Lambda
    /// </summary>
    public float CreditsUsed { get; set; }
    public float HoldProfit { get; set; }
    public float HighProfit { get; set; }
    [DynamoDBGlobalSecondaryIndexHashKey]
    public string RequestDetails { get; set; }
    public string S3ObjectName { get; set; }
}
