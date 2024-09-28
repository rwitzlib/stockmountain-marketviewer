using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.ScanV2.Operands;

[ExcludeFromCodeCoverage]
public class FixedOperand : IScanOperand
{
    public float Value { get; set; }

    public int GetPriority()
    {
        return 100;
    }

    public bool HasTimeframe(out int? multiplier, out Timespan? timespan)
    {
        multiplier = null;
        timespan = null;
        return false;
    }
}
