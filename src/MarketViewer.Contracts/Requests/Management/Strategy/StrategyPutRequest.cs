using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Enums.Strategy;
using MarketViewer.Contracts.Models.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Management.Strategy;

[ExcludeFromCodeCoverage]
public class StrategyPutRequest
{
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public bool IsPublic { get; set; }
    public TradeType Type { get; set; }
    public IntegrationType Integration { get; set; }
    public BacktestPositionInformation PositionInfo { get; set; }
    public ExitInformationDto ExitInfo { get; set; }
    public ScanArgumentDto Argument { get; set; }
}
