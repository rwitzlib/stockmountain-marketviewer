using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests;

[ExcludeFromCodeCoverage]
public class ScanRequestV2 : IRequest<OperationResult<ScanResponse>>
{
    public ScanArgument Argument { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
