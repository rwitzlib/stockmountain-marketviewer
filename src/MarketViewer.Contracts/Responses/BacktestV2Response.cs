using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Models.BacktestV2;

namespace MarketViewer.Contracts.Responses
{
    [ExcludeFromCodeCoverage]
    public class BacktestV2Response
    {
        public Guid RequestId { get; set; }
        public BackTestEntryStats Hold { get; set; }
        public BackTestEntryStats High { get; set; }
        public BackTestEntryStats Other { get; set; }
        public IEnumerable<BacktestEntryV2> Results { get; set; }
    }
}
