using System;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Responses
{
    [ExcludeFromCodeCoverage]
    public class BacktestEntry
    {
        public Guid EntryId { get; set; }
        public DateTime Date { get; set; }
        public double LongRatio { get; set; }
        public double ShortRatio { get; set; }
        public float LongPositionChange { get; set; }
        public float ShortPositionChange { get; set; }
    }
}