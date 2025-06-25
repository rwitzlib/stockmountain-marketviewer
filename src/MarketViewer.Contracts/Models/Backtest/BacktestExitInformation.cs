using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestExitInformation
{
    public Exit StopLoss { get; set; }
    public Exit ProfitTarget { get; set; }
    public ScanArgumentDto Other { get; set; }
    public Timeframe Timeframe { get; set; }
}
