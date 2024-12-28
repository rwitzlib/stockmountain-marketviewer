using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketViewer.Core.Config;

[ExcludeFromCodeCoverage]
public class ServiceConfigs
{
    public string BacktestOrchestrator { get; set; }
    public string BacktestStore { get; set; }
}
