using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.ScanV2;

[ExcludeFromCodeCoverage]
public class ValueOperand : IScanOperand
{
    public string Name { get; } = "Value";
    public float Value { get; set; }

    public bool HasTimeframe(out int? multiplier, out Timespan? timespan)
    {
        multiplier = null;
        timespan = null;
        return false;
    }
}
