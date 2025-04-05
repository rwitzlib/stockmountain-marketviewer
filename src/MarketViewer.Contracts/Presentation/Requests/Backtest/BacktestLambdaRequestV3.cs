using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Presentation.Models;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestLambdaRequestV3
{
    public DateTimeOffset Date { get; set; }
    public BacktestPositionInformation PositionInfo { get; set; }
    public BacktestExitInformation Exit { get; set; }
    public IEnumerable<Feature> Features { get; set; }
    public ScanArgumentDetails Argument { get; set; }
}
