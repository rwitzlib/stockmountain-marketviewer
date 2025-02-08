using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Contracts.Responses.Backtest;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestRequest : BaseRequest, IRequest<OperationResult<BacktestResponse>>
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
