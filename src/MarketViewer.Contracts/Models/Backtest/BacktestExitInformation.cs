﻿using MarketViewer.Contracts.Models.Scan;
using System.Diagnostics.CodeAnalysis;
using ValueType = MarketViewer.Contracts.Enums.ValueType;

namespace MarketViewer.Contracts.Models.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestExitInformation
{
    public Exit StopLoss { get; set; }
    public Exit ProfitTarget { get; set; }
    public ScanArgument Other { get; set; }
    public Timeframe Timeframe { get; set; }

    public class Exit
    {
        public ValueType Type { get; set; }
        public float Value { get; set; }
    }
}
