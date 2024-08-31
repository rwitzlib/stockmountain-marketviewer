using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.ScanV2.Operands;

[ExcludeFromCodeCoverage]
public class PropertyOperand : IScanOperand
{
    public string Property { get; set; }

    public bool HasTimeframe(out int? multiplier, out Timespan? timespan)
    {
        multiplier = null;
        timespan = null;
        return false;
    }
}
