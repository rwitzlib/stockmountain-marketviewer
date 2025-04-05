using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Presentation.Requests;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Requests.Tools;

[ExcludeFromCodeCoverage]
public class ToolsAggregateRequest : BaseRequest
{
    public string Ticker { get; set; }
    public Timespan Timespan { get; set; }
}
