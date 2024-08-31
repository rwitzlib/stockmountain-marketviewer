using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.BacktestV2;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests;

[ExcludeFromCodeCoverage]
public class BacktestRequest : IRequest<OperationResult<BacktestResponse>>
{
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public string ExitStrategy { get; set; }
    public string ExitType { get; set; }
    public float PositionSize { get; set; }
    public int Candles { get; set; }
    public IEnumerable<Filter> Filters { get; set; }
    public IEnumerable<Feature> Features { get; set; }
}
