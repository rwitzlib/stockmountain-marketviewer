using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.ScanV2;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class BacktesterLambdaV3Request
{
    public DateTimeOffset Date { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public BacktestPosition PositionInfo { get; set; }
    public BacktestExit Exit { get; set; }
    public IEnumerable<Feature> Features { get; set; }
    public ScanArgument Argument { get; set; }
}
