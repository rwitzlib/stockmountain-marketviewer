using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketViewer.Infrastructure.Config;

[ExcludeFromCodeCoverage]
public class ServiceConfigs
{
    public string BacktestOrchestratorLambdaName { get; set; }
    public string BacktestTableName { get; set; }
}
