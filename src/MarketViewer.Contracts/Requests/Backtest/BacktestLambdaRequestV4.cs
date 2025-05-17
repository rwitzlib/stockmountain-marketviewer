using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestLambdaRequestV4
{
    public DateTimeOffset Date { get; set; }
    public BacktestPositionInformation PositionInfo { get; set; }
    public BacktestExitInformation Exit { get; set; }
    public IEnumerable<Feature> Features { get; set; }
    public ScanArgumentDto Argument { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public bool IncludeSnapshot { get; set; } = false;
}
