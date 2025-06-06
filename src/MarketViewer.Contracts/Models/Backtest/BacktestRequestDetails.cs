using MarketViewer.Contracts.Dtos;

using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestRequestDetails
{
    public BacktestPositionInformation PositionInfo { get; set; }
    public BacktestExitInformation Exit { get; set; }
    public ScanArgumentDto Argument { get; set; }
}
