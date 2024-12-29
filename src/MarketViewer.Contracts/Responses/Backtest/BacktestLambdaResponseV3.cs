using MarketViewer.Contracts.Models.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Responses.Backtest;

/// <summary>
/// TODO
/// </summary>
[ExcludeFromCodeCoverage]
public class BacktestLambdaResponseV3
{
    public Guid EntryId { get; set; }
    public DateTime Date { get; set; }

    /// <summary>
    /// Cost is $0.000133334 per credit assuming 2048 MB Lambda
    /// </summary>
    public float CreditsUsed { get; set; }
    public BacktestEntryStats Hold { get; set; }
    public BacktestEntryStats High { get; set; }
    public BacktestEntryStats Other { get; set; }
    public List<BacktestEntryResultCollection> Results { get; set; }
}