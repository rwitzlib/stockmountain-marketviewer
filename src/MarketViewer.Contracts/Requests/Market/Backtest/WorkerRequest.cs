using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Models.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Market.Backtest;

[ExcludeFromCodeCoverage]
public class WorkerRequest
{
    public DateTimeOffset Date { get; set; }
    public BacktestPositionInformation PositionInfo { get; set; }
    public BacktestExitInformation Exit { get; set; }
    public IEnumerable<Feature> Features { get; set; }
    public ScanArgumentDto Argument { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public bool IncludeSnapshot { get; set; } = false;
}
