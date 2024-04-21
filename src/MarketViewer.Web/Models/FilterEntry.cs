using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Models;

namespace MarketViewer.Web.Models
{
    [ExcludeFromCodeCoverage]
    public class FilterEntry
    {
        public FilterEntry(bool enabled, Filter filter)
        {
            Enabled = enabled;
            Filter = filter;
        }

        public bool Enabled { get; set; }
        public Filter Filter { get; set; }
    }
}
