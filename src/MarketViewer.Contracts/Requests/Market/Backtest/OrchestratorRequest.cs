using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Models.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Market.Backtest;

[ExcludeFromCodeCoverage]
public class OrchestratorRequest
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public BacktestPositionInformation PositionInfo { get; set; }
    public BacktestExitInformation Exit { get; set; }
    public IEnumerable<Feature> Features { get; set; }
    public ScanArgumentDto Argument { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public bool IncludeSnapshot { get; set; } = false;
}
