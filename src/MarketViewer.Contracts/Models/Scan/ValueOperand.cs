using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

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
