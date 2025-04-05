using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Presentation.Requests;
using MarketViewer.Contracts.Presentation.Responses.Tools;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Requests.Tools;

[ExcludeFromCodeCoverage]
public class ToolsScanRequest : BaseRequest, IRequest<OperationResult<ToolsScanResponse>>
{
    public string Ticker { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public ScanArgument Argument { get; set; }
}
