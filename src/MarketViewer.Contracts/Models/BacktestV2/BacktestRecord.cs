using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketViewer.Contracts.Models.BacktestV2;

[ExcludeFromCodeCoverage]
[DynamoDBTable("lad-dev-marketviewer-backtest-store")]
public class BacktestRecord
{
    [DynamoDBHashKey]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [DynamoDBGlobalSecondaryIndexHashKey]
    public string CustomerId { get; set; }
    public string Date { get; set; }
    public float CreditsUsed { get; set; }
    public float HoldProfit { get; set; }
    public float HighProfit { get; set; }
    public string Request { get; set; }
    public string Response { get; set; }
}
