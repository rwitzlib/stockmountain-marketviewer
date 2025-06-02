using Amazon;
using Amazon.Lambda;
using FluentValidation;
using MarketViewer.Application.Handlers.Management;
using MarketViewer.Application.Handlers.Market.Backtest;
using MarketViewer.Application.Handlers.Tools;
using MarketViewer.Application.Validators;
using MarketViewer.Contracts.Requests.Market.Backtest;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MarketViewer.Application.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterApplication(this IServiceCollection services)
    {
        return services.AddScoped<IValidator<BacktestCreateRequest>, BacktestRequestValidator>()
            .AddSingleton<IAmazonLambda, AmazonLambdaClient>(client => new AmazonLambdaClient(new AmazonLambdaConfig
            {
                Timeout = TimeSpan.FromMinutes(5),
                RegionEndpoint = RegionEndpoint.USEast2
            }))
            .AddScoped<StrategyHandler>()
            .AddScoped<UserHandler>()
            .AddScoped<TradeHandler>()
            .AddScoped<SnapshotHandler>()
            .AddScoped<BacktestHandler>();
    }
}
