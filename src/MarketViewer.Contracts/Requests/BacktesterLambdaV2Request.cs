using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Models.Scan;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests;

[ExcludeFromCodeCoverage]
public class BacktesterLambdaV2Request
{
    public DateTimeOffset Timestamp { get; set; }
    public string ExitStrategy { get; set; }
    public string ExitType { get; set; }
    public float PositionSize { get; set; }
    public int Candles { get; set; }
    public ScanArgument Argument { get; set; }
    public IEnumerable<Feature> Features { get; set; }
}
