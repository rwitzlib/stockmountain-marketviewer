using MarketViewer.Contracts.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Responses
{
    [ExcludeFromCodeCoverage]
    public class StrategyResponse
    {
        public IEnumerable<StrategyEntry> Results { get; set; }
        public int ResultsCount { get; set; }
    }
}
