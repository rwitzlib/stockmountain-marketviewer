using MarketViewer.Application.DependencyInjection;
using MarketViewer.Infrastructure.DependencyInjection;
using MarketViewer.Api.Hubs;
using System.Diagnostics.CodeAnalysis;
using MarketViewer.Infrastructure.Mapping;
using MarketViewer.Application.Mapping;
using MarketViewer.Core.DependencyInjection;
using MarketViewer.Api.HostedServices;
using Quartz;
using MarketViewer.Api.Jobs;
using MarketViewer.Application.Handlers;
using MarketViewer.Contracts.Converters;
using MarketViewer.Api.Binders;
using System.Text.Json.Serialization;

namespace MarketViewer.Api;

public class Program
{
    [ExcludeFromCodeCoverage]
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        DotNetEnv.Env.Load($"{builder.Environment.EnvironmentName}.env");
        builder.Configuration.AddEnvironmentVariables();

        // Add services to the container.
        var microserviceApplicationAssemblies = new[]
        {
            typeof(StocksHandler).Assembly,
            typeof(FilterProfile).Assembly,
            typeof(AggregateProfile).Assembly
        };

        builder.Services.AddQuartz();
        builder.Services.AddQuartzHostedService(opt =>
        {
            opt.WaitForJobsToComplete = true;
        });

        builder.Services.AddMediatR(q => q.RegisterServicesFromAssemblies(microserviceApplicationAssemblies))
            .AddAutoMapper(microserviceApplicationAssemblies);

        builder.Services.AddMemoryCache()
            .AddHostedService<PopulateMarketData>()
            .AddSignalR();

        builder.Services.RegisterApplication()
            .RegisterCore()
            .RegisterInfrastructure(builder.Configuration);

        var now = DateTimeOffset.Now;
        var startTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 0, 1, now.Offset);

        var aggregatesJob = JobBuilder.Create<StocksJob>()
                .WithIdentity("aggregates", "group1")
                .Build();

        var snapshotJob = JobBuilder.Create<SnapshotJob>()
            .WithIdentity("snapshot", "group2")
            .Build();

        var aggregatesTrigger = TriggerBuilder.Create()
            .WithIdentity("trigger1", "group1")
            .StartAt(startTime)
            .ForJob(aggregatesJob)
            .Build();

        var snapshotTrigger = TriggerBuilder.Create()
            .WithIdentity("trigger2", "group2")
            .StartAt(startTime.AddMinutes(1))
            .WithSimpleSchedule(schedule => schedule
                .WithIntervalInMinutes(1)
                .RepeatForever())
            .ForJob(snapshotJob)
            .Build();

        builder.Services.AddControllers(options =>
        {
            options.ModelBinderProviders.Insert(0, new BinderProvider());
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
            options.JsonSerializerOptions.Converters.Add(new ScanArgumentConverter());
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
        var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();

        scheduler.ScheduleJob(aggregatesJob, aggregatesTrigger).GetAwaiter().GetResult();
        scheduler.ScheduleJob(snapshotJob, snapshotTrigger).GetAwaiter().GetResult();

        // Configure the HTTP request pipeline.
        if (IsDevelopment(app.Environment.EnvironmentName))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(policy => policy
           .AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());

        app.UseHttpsRedirection();

        app.UseWebSockets();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(q => q.MapHub<ChatHub>("/chathub"));
        app.MapControllers();

        app.Run();
    }

    private static bool IsDevelopment(string environment)
    {
        return environment switch
        {
            "docker" => true,
            "local" => true,
            "dev" => true,
            "qa" => false,
            "cert" => false,
            "prod" => false,
            _ => false
        };
    }
}