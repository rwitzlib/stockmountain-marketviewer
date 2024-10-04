using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Responses;
using MediatR;

namespace MarketViewer.Contracts.Requests.Scan
{
    [ExcludeFromCodeCoverage]
    public class ScanRequest : IRequest<OperationResult<ScanResponse>>
    {
        public IEnumerable<Filter> Filters { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }
}
