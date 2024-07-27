using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Models.BacktestV2;

[ExcludeFromCodeCoverage]
public class BacktestEntryV2
{
    public Guid EntryId { get; set; }
    public DateTime Date { get; set; }
    public double LongRatio { get; set; }
    public BackTestEntryStats Hold { get; set; }
    public BackTestEntryStats High { get; set; }
    public BackTestEntryStats Average { get; set; }
    public List<List<BackTestEntryResult>> Results { get; set; } 
}

public class BackTestEntryStats
{
    public float PositiveTrendRatio { get; set; }
    public float HighPosition { get; set; }
    public float LowPosition { get; set; }
    public float AvgPosition { get; set; }
    public float SumProfit { get; set; }
}

public class BackTestEntryResult
{
    public string Name { get; set; }
    public string Ticker { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public float StartPrice { get; set; }
    public float ExitPrice { get; set; }
    public float Position { get; set; }
    public int Shares { get; set; }
}