using MarketViewer.Contracts.Requests;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Api.Binders.Providers;

[ExcludeFromCodeCoverage]
public class AggregateBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context is null)
        {
            return null;
        }

        if (context.Metadata.ModelType == typeof(StocksRequest))
        {
            return new AggregateModelBinder();
        }
        else if (context.Metadata.ModelType == typeof(ScanRequestV2))
        {
            return new ScanRequestBinder();
        }

        return null;
    }
}