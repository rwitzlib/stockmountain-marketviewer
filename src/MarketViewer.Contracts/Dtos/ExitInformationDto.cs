using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;
using static MarketViewer.Contracts.Models.Backtest.BacktestExitInformation;

namespace MarketViewer.Contracts.Dtos;

[ExcludeFromCodeCoverage]
public class ExitInformationDto
{
    public Exit StopLoss { get; set; }
    public Exit ProfitTarget { get; set; }
    public ScanArgumentDto Other { get; set; }
    public Timeframe Timeframe { get; set; }
}
