using MarketViewer.Contracts.Enums;
using MarketViewer.Contracts.Models.BacktestV2;
using MarketViewer.Contracts.Models.ScanV2;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests;

[ExcludeFromCodeCoverage]
public class BacktesterLambdaV2Request
{
    public DateTimeOffset Timestamp { get; set; }
    public bool DetailedResponse { get; set; } = false;
    public float PositionSize { get; set; }
    public Timespan Timespan { get; set; }
    public int Multiplier { get; set; }
    public IEnumerable<string> Tickers { get; set; }
    public ScanArgument Argument { get; set; }
    public IEnumerable<Feature> Features { get; set; }
}
