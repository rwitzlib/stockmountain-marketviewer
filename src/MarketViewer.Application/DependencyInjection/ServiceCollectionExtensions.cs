using Amazon;
using Amazon.Lambda;
using FluentValidation;
using MarketViewer.Application.Validators;
using MarketViewer.Contracts.Requests;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Application.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterApplication(this IServiceCollection services)
    {
        return services.AddScoped<IValidator<BacktestRequest>, BacktestRequestValidator>()
            .AddSingleton<IAmazonLambda, AmazonLambdaClient>(client => new AmazonLambdaClient(new AmazonLambdaConfig
            {
                RegionEndpoint = RegionEndpoint.USEast2
            }));
    }
}
