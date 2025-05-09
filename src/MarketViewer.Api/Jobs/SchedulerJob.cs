using Microsoft.Extensions.Caching.Memory;
using Quartz;

namespace MarketViewer.Api.Jobs;

public class SchedulerJob(ISchedulerFactory schedulerFactory, IMemoryCache memoryCache, ILogger<ISchedulerFactory> logger) : IJob
{
    private readonly TimeZoneInfo TimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("SchedulerJob started at: {time}.", DateTimeOffset.Now);

        (memoryCache as MemoryCache).Clear();
        GC.Collect();

        var scheduler = await schedulerFactory.GetScheduler();

        var tickerJob = JobBuilder.Create<TickerInfoJob>()
            .Build();

        var tickerTrigger = TriggerBuilder.Create()
            .StartNow()
            .ForJob(tickerJob)
            .Build();

        await scheduler.ScheduleJob(tickerJob, tickerTrigger);

        var schedulerJob = JobBuilder.Create<SchedulerJob>()
            .Build();

        // Schedule the job to run at 8:00 AM the next day
        var now = DateTimeOffset.Now;
        TimeSpan offset = TimeZone.GetUtcOffset(DateTimeOffset.Now.AddDays(1));
        var startDate = new DateTimeOffset(now.Year, now.Month, now.Day + 1, 8, 0, 1, offset);
        logger.LogInformation("SchedulerJob running next at: {time}.", startDate);
        var scheduleTrigger = TriggerBuilder.Create()
            .StartAt(startDate)
            .ForJob(schedulerJob)
            .Build();

        await scheduler.ScheduleJob(schedulerJob, scheduleTrigger);
    }
}