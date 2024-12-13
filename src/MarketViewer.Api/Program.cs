using MarketViewer.Application.DependencyInjection;
using MarketViewer.Infrastructure.DependencyInjection;
using MarketViewer.Api.Hubs;
using System.Diagnostics.CodeAnalysis;
using MarketViewer.Infrastructure.Mapping;
using MarketViewer.Application.Mapping;
using MarketViewer.Core.DependencyInjection;
using Quartz;
using MarketViewer.Api.Jobs;
using MarketViewer.Application.Handlers;
using MarketViewer.Contracts.Converters;
using MarketViewer.Api.Binders;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MarketViewer.Api.Healthcheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarketViewer.Api;

public class Program
{
    [ExcludeFromCodeCoverage]
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        DotNetEnv.Env.Load($"../../../../{builder.Environment.EnvironmentName}.env");
        builder.Configuration.AddEnvironmentVariables();

        var microserviceApplicationAssemblies = new[]
        {
            typeof(StocksHandler).Assembly,
            typeof(FilterProfile).Assembly,
            typeof(AggregateProfile).Assembly
        };

        builder.Services.AddMediatR(q => q.RegisterServicesFromAssemblies(microserviceApplicationAssemblies))
            .AddAutoMapper(microserviceApplicationAssemblies)
            .AddMemoryCache()
            .RegisterApplication()
            .RegisterCore()
            .RegisterInfrastructure(builder.Configuration)
            .AddSignalR();

        var jobs = builder.Services.AddQuartz()
            .AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
            })
            .RegisterMarketDataJobs();

        builder.Services.AddControllers(options =>
        {
            options.ModelBinderProviders.Insert(0, new BinderProvider());
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
            options.JsonSerializerOptions.Converters.Add(new ScanArgumentConverter());
            options.JsonSerializerOptions.Converters.Add(new FilterConverter());
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks()
            .AddCheck<PingHealthCheck>("Ping", tags: ["healthcheck"]);

        var app = builder.Build();

        var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();

        foreach (var (jobDetail, jobTrigger) in jobs)
        {
            await scheduler.ScheduleJob(jobDetail, jobTrigger);
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsEnvironment("dev") || app.Environment.IsEnvironment("local"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(policy => policy
           .AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());

        app.UseHttpsRedirection();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = q => q.Tags.Contains("healthcheck"),
            ResponseWriter = async (context, report) =>
            {
                var result = report.Entries.All(check => check.Value.Status == HealthStatus.Healthy);
                await context.Response.WriteAsync(result ? "Healthy" : "Unhealthy");
            }
        });

        app.UseWebSockets();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(q => q.MapHub<ChatHub>("/chathub"));
        app.MapControllers();

        app.Run();
    }
}