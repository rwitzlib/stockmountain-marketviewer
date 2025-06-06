using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.Scan;
using System.Collections.Generic;
using System.Linq;

namespace MarketViewer.Infrastructure.Utilities;

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
            if (filter.FirstOperand.HasTimeframe(out var firstTimeframe))
            {
                timespans.Add(firstTimeframe.Timespan);
            }
            if (filter.SecondOperand.HasTimeframe(out var secondTimeframe))
            {
                timespans.Add(secondTimeframe.Timespan);
            }
        }

        var timeSpansFromArgument = GetTimespans(scanArgument.Argument);

        timespans.AddRange(timeSpansFromArgument);

        return timespans.Distinct();
    }
}
