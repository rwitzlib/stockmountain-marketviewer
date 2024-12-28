using MarketViewer.Contracts.Enums.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Responses.Backtest;

[ExcludeFromCodeCoverage]
public class StartBacktestResponse
{
    public string Id { get; set; }
    public BacktestStatus Status { get; set; }
}