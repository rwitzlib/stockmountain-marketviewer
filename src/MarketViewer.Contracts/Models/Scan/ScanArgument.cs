using MarketViewer.Contracts.Converters;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Models.Scan;

[JsonConverter(typeof(ScanArgumentConverter))]
public class ScanArgument
{
    public string Operator { get; set; }
    public List<FilterV2> Filters { get; set; }
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
            if (filter.FirstOperand.HasTimeframe(out var firstMultiplier, out var firstTimespan))
            {
                timeFrames.Add(new Timeframe(firstMultiplier.Value, firstTimespan.Value));
            }
            if (filter.SecondOperand.HasTimeframe(out var secondMultiplier, out var secondTimespan))
            {
                timeFrames.Add(new Timeframe(secondMultiplier.Value, secondTimespan.Value));
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
            if (filter.FirstOperand.HasTimeframe(out var firstMultiplier, out var firstTimespan))
            {
                timeFrames.Add(new Timeframe(firstMultiplier.Value, firstTimespan.Value));
            }
            if (filter.SecondOperand.HasTimeframe(out var secondMultiplier, out var secondTimespan))
            {
                timeFrames.Add(new Timeframe(secondMultiplier.Value, secondTimespan.Value));
            }
        }

        if (argument.Argument is not null)
        {
            timeFrames.AddRange(GetInternalTimeframes(argument.Argument));
        }

        return timeFrames;
    }
}
