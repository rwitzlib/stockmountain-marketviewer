using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;
using ValueType = MarketViewer.Contracts.Enums.ValueType;

namespace MarketViewer.Contracts.Models;

[ExcludeFromCodeCoverage]
public class Exit
{
    public ExitCandleType CandleType { get; set; }
    public PriceActionType PriceActionType { get; set; }
    public ValueType Type { get; set; }
    public float Value { get; set; }
}
