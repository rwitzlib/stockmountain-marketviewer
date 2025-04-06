using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Entities.Scan;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

[JsonConverter(typeof(ScanArgumentConverter))]
public class ScanArgument
{
    public string Operator { get; set; }
    public List<Filter> Filters { get; set; }
    public ScanArgument Argument { get; set; }

    public List<Timeframe> GetTimeframes()
    {
        if (this is null)
        {
            return [];
        }

        var timeFrames = new List<Timeframe>();

        foreach (var filter in Filters)
        {
            if (filter.FirstOperand.HasTimeframe(out var firstTimeframe))
            {
                timeFrames.Add(firstTimeframe);
            }
            if (filter.SecondOperand.HasTimeframe(out var secondTimeframe))
            {
                timeFrames.Add(secondTimeframe);
            }
        }

        if (Argument is not null)
        {
            timeFrames.AddRange(GetInternalTimeframes(Argument));

        }

        return timeFrames.DistinctBy(q => (q.Multiplier, q.Timespan)).ToList();
    }

    private static List<Timeframe> GetInternalTimeframes(ScanArgument argument)
    {
        List<Timeframe> timeFrames = [];
        if (argument is null)
        {
            return timeFrames;
        }

        foreach (var filter in argument.Filters)
        {
            if (filter.FirstOperand.HasTimeframe(out var firstTimeframe))
            {
                timeFrames.Add(firstTimeframe);
            }
            if (filter.SecondOperand.HasTimeframe(out var secondTimeframe))
            {
                timeFrames.Add(secondTimeframe);
            }
        }

        if (argument.Argument is not null)
        {
            timeFrames.AddRange(GetInternalTimeframes(argument.Argument));
        }

        return timeFrames;
    }
}
