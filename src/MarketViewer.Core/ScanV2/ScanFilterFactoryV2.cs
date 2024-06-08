using MarketViewer.Contracts.Models;
using MarketViewer.Contracts.Models.Scan;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using IFilterV2 = MarketViewer.Core.Scanner.Filters.IFilterV2;

namespace MarketViewer.Core.ScanV2
{
    [ExcludeFromCodeCoverage]
    public class ScanFilterFactoryV2(IServiceProvider serviceProvider)
    {
        public IFilterV2 GetFilterForOperand(IScanOperand operand)
        {
            return operand.GetType().ToString() switch
            {
                "PriceActionOperand" => serviceProvider.GetRequiredService<IFilterV2>(),
                _ => throw new NotImplementedException()
            } ;
        }
    }
}
