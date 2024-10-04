using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.Scan;

namespace MarketViewer.Web.Contracts
{
    [ExcludeFromCodeCoverage]
    public class FilterParams
    {
        public FilterType FilterType { get; set; }
        public FilterTypeModifier FilterTypeModifier { get; set; }
        public FilterOperator Operator { get; set; }
        public FilterValueType ValueType { get; set; }
        public string Value { get; set; }
        public string Multiplier { get; set; }
        public Timespan Timespan { get; set; }
    }
}
