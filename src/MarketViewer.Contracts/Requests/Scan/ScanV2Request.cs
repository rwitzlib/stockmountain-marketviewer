using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses;
using MediatR;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Scan;

[ExcludeFromCodeCoverage]
public class ScanV2Request : IRequest<OperationResult<ScanResponse>>
{
    public ScanArgument Argument { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
