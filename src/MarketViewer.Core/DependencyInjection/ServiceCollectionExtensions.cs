using MarketViewer.Core.Scanner;
using MarketViewer.Core.Scanner.Filters;
using MarketViewer.Core.ScanV2;
using MarketViewer.Core.ScanV2.Filters;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Core.DependencyInjection
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCore(this IServiceCollection services)
        {
            services.AddSingleton<ScanFilterFactory>()
                .AddSingleton<FloatFilter>()
                .AddSingleton<MacdFilter>()
                .AddSingleton<PriceFilter>()
                .AddSingleton<VolumeFilter>()
                .AddSingleton<VwapFilter>();

            services.AddSingleton<ScanFilterFactoryV2>()
                .AddSingleton<PriceActionFilter>()
                .AddSingleton<StudyFilter>()
                .AddSingleton<ValueFilter>();

            return services;
        }
    }
}
