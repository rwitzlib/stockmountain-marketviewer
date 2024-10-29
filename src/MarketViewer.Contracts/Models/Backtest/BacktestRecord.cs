using Amazon.DynamoDBv2.DataModel;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Backtest;

[ExcludeFromCodeCoverage]
[DynamoDBTable("lad-dev-marketviewer-backtest-store")]
public class BacktestRecord
{
    [DynamoDBHashKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [DynamoDBGlobalSecondaryIndexHashKey]
    public string CustomerId { get; set; }
    public string Date { get; set; }

    /// <summary>
    /// Cost is $0.000133334 per credit
    /// </summary>
    public float CreditsUsed { get; set; }
    public float HoldProfit { get; set; }
    public float HighProfit { get; set; }
    [DynamoDBGlobalSecondaryIndexHashKey]
    public string RequestDetails { get; set; }
    public int StartDate { get; set; }
    public int EndDate { get; set; }
    public string S3ObjectName { get; set; }
}
