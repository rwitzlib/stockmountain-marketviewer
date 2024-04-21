using MarketViewer.Contracts.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests;

[ExcludeFromCodeCoverage]
public class BacktesterLambdaRequest
{
    public DateTimeOffset Timestamp { get; set; }
    public string ExitStrategy { get; set; }
    public string ExitType { get; set; }
    public float PositionSize { get; set; }
    public int Candles { get; set; }
    public IEnumerable<Filter> Filters { get; set; }
    public IEnumerable<Feature> Features { get; set; }
}
