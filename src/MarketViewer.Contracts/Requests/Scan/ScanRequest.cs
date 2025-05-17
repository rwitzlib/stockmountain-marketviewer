using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Requests;
using MarketViewer.Contracts.Responses;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Scan;

[ExcludeFromCodeCoverage]
public class ScanRequest : BaseRequest, IRequest<OperationResult<ScanResponse>>
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
    public ScanArgumentDto Argument { get; set; }
}
