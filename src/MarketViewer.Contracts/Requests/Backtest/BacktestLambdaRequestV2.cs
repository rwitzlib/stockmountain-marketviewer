using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.ScanV2;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestLambdaRequestV2
{
    public DateTimeOffset Timestamp { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public float PositionSize { get; set; }
    public Timespan Timespan { get; set; }
    public int Multiplier { get; set; }
    public float StopLoss { get; set; }
    public int MaxPositions { get; set; }
    public IEnumerable<string> Tickers { get; set; }
    public ScanArgument Argument { get; set; }
    public IEnumerable<Feature> Features { get; set; }
}
