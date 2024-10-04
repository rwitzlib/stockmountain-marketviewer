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
using MarketViewer.Contracts.Enums;

namespace MarketViewer.Api;

public class Program
{
    [ExcludeFromCodeCoverage]
    private static async Task Main(string[] args)
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

        builder.Services.AddMediatR(q => q.RegisterServicesFromAssemblies(microserviceApplicationAssemblies))
            .AddAutoMapper(microserviceApplicationAssemblies)
            .AddMemoryCache()
            .AddSignalR(); 
        
        builder.Services.AddQuartz()
            .AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
            })
            .RegisterApplication()
            .RegisterCore()
            .RegisterInfrastructure(builder.Configuration);

        var now = DateTimeOffset.Now;
        var minuteStartTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 1, 0, now.Offset);
        // Start at 9:01, 10:01, etc. to get the minute before: 9:00, 10:00, etc.
        var hourStartTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.AddHours(1).Hour, 1, 1, 0, now.Offset);

        var initJob = JobBuilder.Create<InitializeJob>()
            .WithIdentity("ticker")
            .UsingJobData("date", DateTimeOffset.Now.ToString())
            .Build();

        var snapshotMinuteJob = JobBuilder.Create<SnapshotJob>()
            .WithIdentity("snapshotMinute")
            .UsingJobData("timespan", Timespan.minute.ToString())
            .Build();

        var snapshotHourJob = JobBuilder.Create<SnapshotJob>()
            .WithIdentity("snapshotHour")
            .UsingJobData("timespan", Timespan.hour.ToString())
            .Build();

        var initTrigger = TriggerBuilder.Create()
            .WithIdentity("trigger1")
            .StartNow()
            .ForJob(initJob)
            .Build();
        
        var snapshopMinuteTrigger = TriggerBuilder.Create()
            .WithIdentity("trigger2")
            .StartAt(minuteStartTime)
            .WithSimpleSchedule(schedule => schedule
                .WithIntervalInMinutes(1)
                .RepeatForever())
            .ForJob(snapshotMinuteJob)
            .Build();

        var snapshotHourTrigger = TriggerBuilder.Create()
            .WithIdentity("trigger3")
            .StartAt(hourStartTime)
            .WithSimpleSchedule(schedule => schedule
                .WithIntervalInHours(1)
                .RepeatForever())
            .ForJob(snapshotHourJob)
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
        var scheduler = await schedulerFactory.GetScheduler();

        await scheduler.ScheduleJob(initJob, initTrigger);
        await scheduler.ScheduleJob(snapshotMinuteJob, snapshopMinuteTrigger);
        await scheduler.ScheduleJob(snapshotHourJob, snapshotHourTrigger);

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