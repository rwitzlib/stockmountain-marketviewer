using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.Scan;

[ExcludeFromCodeCoverage]
public class StudyOperand : IScanOperand
{
    public StudyType Type { get; set; }
    public string[] Parameters { get; set; }
    public int Multiplier { get; set; }
    public Timespan Timespan { get; set; }

    public float[] Compute(StocksResponseCollection stocksResponse, Timeframe timeframe)
    {
        return [];
    }

    public bool HasTimespan(out Timespan? timespan)
    {
        timespan = Timespan;
        return true;
    }
}
