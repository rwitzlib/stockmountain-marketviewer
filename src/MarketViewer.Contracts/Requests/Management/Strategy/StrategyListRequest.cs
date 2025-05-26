using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Management.Strategy;

[ExcludeFromCodeCoverage]
public class StrategyListRequest
{
    public bool IsPublic { get; set; }
}
