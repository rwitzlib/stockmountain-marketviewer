using MarketViewer.Contracts.Enums.Scan;
using MarketViewer.Contracts.Models.ScanV2;
using MarketViewer.Core.Scan.Filters;
using MarketViewer.Core.Scanner.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Core.Scan
{
    [ExcludeFromCodeCoverage]
    public class ScanFilterFactory(IServiceProvider serviceProvider)
    {
        public IFilter GetScanFilter(Filter filter)
        {
            return filter.Type switch
            {
                FilterType.Volume => serviceProvider.GetRequiredService<VolumeFilter>(),
                FilterType.Price => serviceProvider.GetRequiredService<PriceFilter>(),
                FilterType.Vwap => serviceProvider.GetRequiredService<VwapFilter>(),
                FilterType.Macd => serviceProvider.GetRequiredService<MacdFilter>(),
                _ => throw new NotImplementedException()
            };
        }
    }
}
