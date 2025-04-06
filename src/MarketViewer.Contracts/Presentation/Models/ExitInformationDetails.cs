using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;
using static MarketViewer.Contracts.Models.Backtest.BacktestExitInformation;

namespace MarketViewer.Contracts.Presentation.Models;

[ExcludeFromCodeCoverage]
public class ExitInformationDetails
{
    public Exit StopLoss { get; set; }
    public Exit ProfitTarget { get; set; }
    public List<FilterDetails> Other { get; set; }
    public Timeframe Timeframe { get; set; }
}
