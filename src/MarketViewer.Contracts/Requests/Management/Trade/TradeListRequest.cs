﻿using MarketViewer.Contracts.Enums.Strategy;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Contracts.Requests.Management.Trade;

[ExcludeFromCodeCoverage]
public class TradeListRequest
{
    public string Strategy { get; set; }
    public string User { get; set; }
    public TradeType? Type { get; set; }
    public TradeStatus? Status { get; set; }
}
