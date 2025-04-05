using MarketViewer.Contracts.Entities.Backtest;
using MarketViewer.Contracts.Enums.Backtest;
using MarketViewer.Contracts.Models.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Responses.Backtest;

[ExcludeFromCodeCoverage]
public class GetBacktestResultResponse
{
    public string Id { get; set; }
    public BacktestStatus Status { get; set; }
    public BacktestEntryStats Hold { get; set; }
    public BacktestEntryStats Other { get; set; }
    public BacktestEntryStats High { get; set; }
    public IEnumerable<BacktestDayResultV3> Results { get; set; }
    public IEnumerable<BacktestLambdaResponseV3> Entries { get; set; }
}