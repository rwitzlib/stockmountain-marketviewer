using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using System.Collections.Generic;
using System.Linq;

namespace MarketViewer.Application.Utilities;

public static class ScanUtilities
{
    public static IEnumerable<Timespan> GetTimespans(ScanArgument scanArgument)
    {
        if (scanArgument == null)
        {
            return [];
        }

        var timespans = new List<Timespan>();

        foreach (var filter in scanArgument.Filters)
        {
            if (filter.FirstOperand.HasTimeframe(out var firstMultiplier, out var firstTimespan))
            {
                timespans.Add(firstTimespan.Value);
            }
            if (filter.SecondOperand.HasTimeframe(out var secondMultiplier, out var secondTimespan))
            {
                timespans.Add(secondTimespan.Value);
            }
        }

        var timeSpansFromArgument = GetTimespans(scanArgument.Argument);

        timespans.AddRange(timeSpansFromArgument);

        return timespans.Distinct();
    }
}
