using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Responses;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.ScanV2;

[ExcludeFromCodeCoverage]
public class StocksResponseCollection
{
    public Dictionary<Timespan, IEnumerable<StocksResponse>> Responses { get; set; } = [];
}
