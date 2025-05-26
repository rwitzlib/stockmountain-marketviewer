using MarketViewer.Application.DependencyInjection;
using MarketViewer.Infrastructure.DependencyInjection;
using MarketViewer.Api.Hubs;
using System.Diagnostics.CodeAnalysis;
using MarketViewer.Infrastructure.Mapping;
using MarketViewer.Core.DependencyInjection;
using Quartz;
using MarketViewer.Api.Jobs;
using MarketViewer.Contracts.Converters;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MarketViewer.Api.Healthcheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MarketViewer.Api.Authentication;
using Microsoft.AspNetCore.HttpLogging;
using MarketViewer.Core.Scan;
using MarketViewer.Studies.DependencyInjection;
using MarketViewer.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using MarketViewer.Application.Handlers.Market;
using MarketViewer.Core.Auth;
using MarketViewer.Api.Middleware;

namespace MarketViewer.Api;

public class Program
{
    [ExcludeFromCodeCoverage]
    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        DotNetEnv.Env.Load($"../../../{builder.Environment.EnvironmentName}.env");
        builder.Configuration.AddEnvironmentVariables();

        var microserviceApplicationAssemblies = new[]
        {
            typeof(StocksHandler).Assembly,
            typeof(AggregateProfile).Assembly,
            typeof(ScanFilterFactoryV2).Assembly
        };

        builder.Services.AddMediatR(q => q.RegisterServicesFromAssemblies(microserviceApplicationAssemblies))
            .AddAutoMapper(microserviceApplicationAssemblies)
            .AddMemoryCache(options => options.TrackStatistics = true)
            .AddHttpClient()
            .RegisterStudies()
            .RegisterApplication()
            .RegisterCore(builder.Configuration)
            .RegisterInfrastructure(builder.Configuration)
            .AddHttpContextAccessor()
            .AddSignalR();

        builder.Services.AddQuartz()
            .AddQuartzHostedService(opt =>
            {
                opt.WaitForJobsToComplete = true;
            });

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.Converters.Add(new ScanArgumentConverter());
            options.JsonSerializerOptions.Converters.Add(new FilterConverter());
            options.JsonSerializerOptions.Converters.Add(new StudyConverter());
        });

        var signingKeyCache = new SigningKeyCache();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "http://auth.stockmountain.io",
                    ValidAudience = "react",
                    IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                    {
                        var keys = signingKeyCache.GetKeys();
                        return new JsonWebKeySet(keys).GetSigningKeys();
                    }
                };
            });

        // Register the authorization handler and policy
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        builder.Services.AddScoped<IAuthorizationHandler, RequiredPermissionsHandler>();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, RequiredPermissionsAuthorizationPolicyProvider>();
        builder.Services.AddAuthorization();

        builder.Services.AddHttpLogging(options => options.LoggingFields = HttpLoggingFields.All);

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks()
            .AddCheck<PingHealthCheck>("Ping", tags: ["healthcheck"]);

        var app = builder.Build();

        var schedulerFactory = app.Services.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler();

        await RegisterJobs(scheduler);

        app.UseHttpLogging();

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

        app.UseAuthentication();
        app.UseMiddleware<AuthContextMiddleware>();
        app.UseAuthorization();

        app.UseEndpoints(q => q.MapHub<ChatHub>("/chathub"));
        app.MapControllers();

        app.Run();
    }

    private static async Task RegisterJobs(IScheduler scheduler)
    {
        var tickerJob = JobBuilder.Create<SchedulerJob>()
           .Build();

        var scheduleTrigger = TriggerBuilder.Create()
            .StartNow()
            .ForJob(tickerJob)
            .Build();

        await scheduler.ScheduleJob(tickerJob, scheduleTrigger);
    }
}
