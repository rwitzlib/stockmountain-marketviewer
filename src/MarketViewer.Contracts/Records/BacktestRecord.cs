using MarketViewer.Contracts.Enums.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Records;

/// <summary>
/// Database record for storing information about backtest results.
/// </summary>
[ExcludeFromCodeCoverage]
public class BacktestRecord
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public BacktestStatus Status { get; set; }
    public string CreatedAt { get; set; }

    /// <summary>
    /// Cost is $0.000133334 per credit assuming 2048 MB Lambda
    /// </summary>
    public float CreditsUsed { get; set; }
    public float HoldProfit { get; set; }
    public float HighProfit { get; set; }
    public float? OtherProfit { get; set; }
    public string RequestDetails { get; set; }
    public string S3ObjectName { get; set; }
    public List<string> Errors { get; set; }
}
