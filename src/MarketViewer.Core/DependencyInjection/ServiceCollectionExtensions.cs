using MarketViewer.Core.Auth;
using MarketViewer.Core.Scan;
using MarketViewer.Core.Scan.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Core.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuthContext>();

        services.AddSingleton<ScanFilterFactoryV2>()
            .AddSingleton<PriceActionFilter>()
            .AddSingleton<StudyFilter>()
            .AddSingleton<ValueFilter>();

        return services;
    }
}
