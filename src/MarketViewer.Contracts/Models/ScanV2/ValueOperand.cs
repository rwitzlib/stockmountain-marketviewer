using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.ScanV2;

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
