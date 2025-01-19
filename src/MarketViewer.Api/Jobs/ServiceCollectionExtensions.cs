using Quartz;

namespace MarketViewer.Api.Jobs;

public static class ServiceCollectionExtensions
{
    public static List<(IJobDetail, ITrigger)> RegisterMarketDataJobs(this IServiceCollection services)
    {
        List<(IJobDetail, ITrigger)> jobs = [];

        var tickerJobKey = JobKey.Create("TickerJob", "Pipeline");

        var now = DateTimeOffset.Now;

        var tickerJob = JobBuilder.Create<TickerInfoJob>()
            .WithIdentity(tickerJobKey)
            .UsingJobData("date", now.ToString())
            .Build();

        var startTime = DateTimeOffset.Now.Second < 30 ? DateTimeOffset.Now : new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 1, now.Offset);

        var initTrigger = TriggerBuilder.Create()
            .WithIdentity("TickerTrigger")
            .StartAt(startTime)
            .ForJob(tickerJob)
            .Build();

        jobs.Add((tickerJob, initTrigger));

        return jobs;
    }
}
