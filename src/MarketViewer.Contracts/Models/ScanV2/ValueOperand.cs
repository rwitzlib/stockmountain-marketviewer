using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.ScanV2;

[ExcludeFromCodeCoverage]
public class ValueOperand : IScanOperand
{
    public string Name { get; } = "Value";
    public float Value { get; set; }

    public bool HasTimespan(out Timespan? timespan)
    {
        timespan = null;
        return false;
    }
}
