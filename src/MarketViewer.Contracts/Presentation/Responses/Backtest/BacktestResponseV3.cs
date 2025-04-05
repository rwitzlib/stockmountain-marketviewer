﻿using MarketViewer.Contracts.Entities.Backtest;
using MarketViewer.Contracts.Models.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Presentation.Responses.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestResponseV3
{
    public string Id { get; set; }

    /// <summary>
    /// Given a 2 GB Lambda, 1 second of backtesting will cost $0.0000333.
    /// So 1 credit is equal to $0.0000333.
    /// Assuming 1 day of backtesting will take 120 seconds, each day of backtesting costs 120 credits or $0.0034 
    /// </summary>
    public float CreditsUsed { get; set; }
    public BacktestEntryStats Hold { get; set; }
    public BacktestEntryStats Other { get; set; }
    public BacktestEntryStats High { get; set; }
    public IEnumerable<BacktestDayResultV3> Results { get; set; }
    public IEnumerable<BacktestLambdaResponseV3> Entries { get; set; }
}