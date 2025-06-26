using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Dtos;

[ExcludeFromCodeCoverage]
public class ExitInformationDto
{
    public Exit StopLoss { get; set; }
    public Exit ProfitTarget { get; set; }
    public ScanArgumentDto Other { get; set; }
    public Timeframe Timeframe { get; set; }
}
