using MarketViewer.Contracts.Converters;
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
public class BacktestV2Request : IRequest<OperationResult<BacktestResponse>>
{
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset End { get; set; }
    public string ExitStrategy { get; set; }
    public string ExitType { get; set; }
    public float PositionSize { get; set; }
    public int Candles { get; set; }

    [JsonConverter(typeof(ScanArgumentConverter))]
    public ScanArgument Argument { get; set; }
    public IEnumerable<Feature> Features { get; set; }
}
