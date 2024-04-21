using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MarketViewer.Contracts.Enums;

namespace MarketViewer.Contracts.Models
{
    [ExcludeFromCodeCoverage]
    public class Filter
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FilterType Type { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FilterTypeModifier Modifier { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FilterOperator Operator { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FilterValueType ValueType { get; set; }

        public float Value { get; set; }

        public int Multiplier { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Timespan Timespan { get; set; }
    }

    public enum FilterType
    {
        Volume,
        Price,
        Vwap,
        Macd,
        Float
    }

    public enum FilterTypeModifier
    {
        Value,
        Slope
    }

    public enum FilterOperator
    {
        gt,
        lt,
        eq,
        ge,
        le
    }

    public enum FilterValueType
    {
        CustomAmount,
        Vwap,
        Macd
    }
}
