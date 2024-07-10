using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Web.Models;

[ExcludeFromCodeCoverage]
public class FilterItem
{
    public string Id { get; set; }
    public string Name { get; set; }

    public FilterItem()
    {
        Id = Guid.NewGuid().ToString();
        Name = $"Filter {Name}";
    }
}
