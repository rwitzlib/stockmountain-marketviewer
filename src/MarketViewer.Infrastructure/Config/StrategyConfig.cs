using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketViewer.Infrastructure.Config;

public class StrategyConfig
{
    public string TableName { get; set; }
    public string PublicIndexName { get; set; }
    public string UserIndexName { get; set; }
}
