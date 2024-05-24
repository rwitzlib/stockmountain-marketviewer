using MarketDataProvider.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class PriceActionOperand : IScanOperand
{
    public int Multiplier { get; set; }
    public Timespan Timespan { get; set; }
}
