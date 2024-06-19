using MarketViewer.Contracts.Models.Scan;
using MarketViewer.Core.ScanV2.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Core.ScanV2;

[ExcludeFromCodeCoverage]
public class ScanFilterFactoryV2(IServiceProvider serviceProvider)
{
    public IFilterV2 GetScanFilter(IScanOperand operand)
    {
        return operand switch
        {
            PriceActionOperand => serviceProvider.GetRequiredService<PriceActionFilter>(),
            StudyOperand => serviceProvider.GetRequiredService<StudyFilter>(),
            ValueOperand => serviceProvider.GetRequiredService<ValueFilter>(),
            _ => throw new NotImplementedException()
        };
    }
}
