using MarketViewer.Contracts.Models.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Responses.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestResponseV3
{
    public string Id { get; set; }
    public BacktestEntryStats Hold { get; set; }
    public BacktestEntryStats Other { get; set; }
    public BacktestEntryStats High { get; set; }
    public IEnumerable<BacktestDayResultV3> Results { get; set; }
    public IEnumerable<BacktestLambdaResponseV3> Entries { get; set; }
}