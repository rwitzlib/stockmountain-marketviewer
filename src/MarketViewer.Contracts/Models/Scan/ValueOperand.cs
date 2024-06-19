using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class ValueOperand : IScanOperand
{
    [JsonRequired]
    public float Value { get; set; }

    public bool HasTimespan(out Timespan? timespan)
    {
        timespan = null;
        return false;
    }
}
