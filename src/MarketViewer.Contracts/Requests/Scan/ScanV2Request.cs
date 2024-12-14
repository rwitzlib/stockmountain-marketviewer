using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Responses;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Scan;

[ExcludeFromCodeCoverage]
public class ScanV2Request : IRequest<OperationResult<ScanResponse>>
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
    public ScanArgument Argument { get; set; }
}
