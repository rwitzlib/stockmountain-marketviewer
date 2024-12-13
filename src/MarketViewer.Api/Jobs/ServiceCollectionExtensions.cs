using Quartz;
using MarketViewer.Contracts.Enums;
using Quartz.Listener;

namespace MarketViewer.Api.Jobs;

public static class ServiceCollectionExtensions
{
    public static List<(IJobDetail, ITrigger)> RegisterMarketDataJobs(this IServiceCollection services)
    {
        List<(IJobDetail, ITrigger)> jobs = [];

        var initJobKey = JobKey.Create("InitJob", "Pipeline");

        var initJob = JobBuilder.Create<InitializeJob>()
            .WithIdentity(initJobKey)
            .UsingJobData("date", DateTimeOffset.Now.ToString())
            .Build();

        var initTrigger = TriggerBuilder.Create()
            .WithIdentity("initTrigger")
            .StartNow()
            .ForJob(initJob)
            .Build();

        jobs.Add((initJob, initTrigger));

        return jobs;
    }
}
