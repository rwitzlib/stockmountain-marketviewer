using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses.Backtest;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestV3Request : IRequest<OperationResult<BacktestV3Response>>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public BacktestPosition PositionInfo { get; set; }
    public BacktestExit Exit { get; set; }
    public IEnumerable<Feature> Features { get; set; }
    public ScanArgument Argument { get; set; }
}
