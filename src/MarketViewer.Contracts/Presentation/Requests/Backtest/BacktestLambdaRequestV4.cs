using MarketViewer.Contracts.Entities.Backtest;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Presentation.Models;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestLambdaRequestV4
{
    public DateTimeOffset Date { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public BacktestPositionInformation PositionInfo { get; set; }
    public BacktestExitInformation Exit { get; set; }
    public IEnumerable<Feature> Features { get; set; }
    public ScanArgumentDetails Argument { get; set; }
}
