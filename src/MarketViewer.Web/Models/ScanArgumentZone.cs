using MarketViewer.Contracts.Models.ScanV2;

namespace MarketViewer.Web.Models;

public class ScanArgumentZone(int depth, string @operator)
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public int Depth { get; set; } = depth;
    public string Operator { get; set; } = @operator;
    public ScanArgumentZone Argument { get; set; }
    public List<FilterItem> Filters { get; set; } = [];

    public List<FilterV2> GetFilters()
    {
        List<FilterV2> filters = [];

        if (Filters is null || !Filters.Any())
        {
            return filters;
        }

        foreach (var filter in Filters)
        {
            filters.Add(new FilterV2
            {
                CollectionModifier = filter.CollectionModifier,
                FirstOperand = filter.FirstOperand,
                Operator = filter.Operator,
                SecondOperand = filter.SecondOperand,
                Timeframe = filter.Timeframe
            });
        }

        return filters;
    }
}
