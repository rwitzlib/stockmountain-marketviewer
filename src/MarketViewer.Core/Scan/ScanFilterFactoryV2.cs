using MarketViewer.Contracts.Models.Scan.Operands;
using MarketViewer.Core.Scan.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Core.Scan;

[ExcludeFromCodeCoverage]
public class ScanFilterFactoryV2(IServiceProvider serviceProvider)
{
    public IFilterV2 GetScanFilter(IScanOperand operand)
    {
        return operand switch
        {
            PriceActionOperand => serviceProvider.GetRequiredService<PriceActionFilter>(),
            StudyOperand => serviceProvider.GetRequiredService<StudyFilter>(),
            FixedOperand => serviceProvider.GetRequiredService<ValueFilter>(),
            PropertyOperand => serviceProvider.GetRequiredService<PropertyFilter>(),
            _ => throw new NotImplementedException()
        };
    }
}
