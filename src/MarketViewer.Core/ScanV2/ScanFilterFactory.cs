using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Core.Scanner.Filters;
using MarketViewer.Core.ScanV2.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using IFilter = MarketViewer.Core.ScanV2.Filters.IFilter;

namespace MarketViewer.Core.ScanV2
{
    [ExcludeFromCodeCoverage]
    public class ScanFilterFactory(IServiceProvider serviceProvider)
    {
        public IFilter GetScanFilter(FilterV2 filter)
        {
            filter.fi
            return filter.Type switch
            {
                FilterType.Volume => serviceProvider.GetRequiredService<VolumeFilter>(),
                FilterType.Price => serviceProvider.GetRequiredService<PriceFilter>(),
                FilterType.Vwap => serviceProvider.GetRequiredService<VwapFilter>(),
                FilterType.Macd => serviceProvider.GetRequiredService<MacdFilter>(),
                FilterType.Float => serviceProvider.GetRequiredService<FloatFilter>(),
                _ => throw new NotImplementedException()
            };
        }
    }
}
