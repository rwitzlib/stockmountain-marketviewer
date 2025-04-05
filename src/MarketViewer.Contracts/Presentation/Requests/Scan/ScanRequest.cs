using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Presentation.Models;
using MarketViewer.Contracts.Presentation.Responses;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Requests.Scan;

[ExcludeFromCodeCoverage]
public class ScanRequest : BaseRequest, IRequest<OperationResult<ScanResponse>>
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
    public ScanArgumentDetails Argument { get; set; }
}
