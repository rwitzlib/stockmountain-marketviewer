using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses.Backtest;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestV2Request : IRequest<OperationResult<BacktestV2Response>>
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public float PositionSize { get; set; }
    public Timespan Timespan { get; set; }
    public int Multiplier { get; set; }
    public float StopLoss { get; set; }
    public int MaxPositions { get; set; }
    public ScanArgument Argument { get; set; }
    public IEnumerable<Feature> Features { get; set; }
}
