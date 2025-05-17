using MarketViewer.Contracts.Enums;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests;

[ExcludeFromCodeCoverage]
public class ToolsAggregateRequest : BaseRequest
{
    public string Ticker { get; set; }
    public Timespan Timespan { get; set; }
}
