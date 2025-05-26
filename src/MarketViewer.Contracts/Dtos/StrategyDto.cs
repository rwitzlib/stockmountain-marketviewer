using MarketViewer.Contracts.Enums.Strategy;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Dtos;

[ExcludeFromCodeCoverage]
public class StrategyDto
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public bool Enabled { get; set; }
    public bool IsPublic { get; set; }
    public TradeType Type { get; set; }
    public IntegrationType Integration { get; set; }
    public BacktestPositionInformation PositionInfo { get; set; }
    public BacktestExitInformation ExitInfo { get; set; }
    public ScanArgument Argument { get; set; }
}
