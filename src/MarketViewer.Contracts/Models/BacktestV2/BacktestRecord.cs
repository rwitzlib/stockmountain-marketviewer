using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketViewer.Contracts.Models.BacktestV2;

[ExcludeFromCodeCoverage]
public class BacktestRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CustomerId { get; set; }
    public float HoldProfit { get; set; }
    public float HighProfit { get; set; }
    public string Request { get; set; }
    public string Response { get; set; }
}
