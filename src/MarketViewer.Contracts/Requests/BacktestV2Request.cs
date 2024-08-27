using MarketViewer.Contracts.Converters;
using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Contracts.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MarketViewer.Contracts.Requests;

[ExcludeFromCodeCoverage]
public class BacktestV2Request : IRequest<OperationResult<BacktestV2Response>>
{
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public float PositionSize { get; set; }
    public Timespan Timespan { get; set; }
    public int Multiplier { get; set; }
    public ScanArgument Argument { get; set; }
    public IEnumerable<Feature> Features { get; set; }
}
