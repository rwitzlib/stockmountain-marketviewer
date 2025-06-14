﻿using MarketViewer.Contracts.Dtos;
using MarketViewer.Contracts.Enums.Backtest;
using MarketViewer.Contracts.Models.Backtest;
using MarketViewer.Contracts.Requests.Market.Backtest;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Responses.Market.Backtest;

[ExcludeFromCodeCoverage]
public class BacktestEntryResponse
{
    public string Id { get; set; }
    public BacktestStatus Status { get; set; }
    public string CreatedAt { get; set; }

    /// <summary>
    /// Cost is $0.000133334 per credit assuming 2048 MB Lambda
    /// </summary>
    public float CreditsUsed { get; set; }
    public float HoldProfit { get; set; }
    public float HighProfit { get; set; }
    public float? OtherProfit { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
    public BacktestRequestDetails RequestDetails { get; set; }
    public float DurationSeconds { get; set; }
    public List<string> Errors { get; set; }
}