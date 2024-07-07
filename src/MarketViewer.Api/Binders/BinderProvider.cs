using MarketViewer.Contracts.Requests;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Api.Binders;

[ExcludeFromCodeCoverage]
public class BinderProvider : IModelBinderProvider
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

        return null;
    }
}