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

        // Schedule the job to run at 9:25 AM EST the next day
        var now = DateTimeOffset.Now;
        var offset = TimeZone.GetUtcOffset(DateTimeOffset.Now.AddDays(1));
        var nextStartDate = new DateTimeOffset(now.Year, now.Month, now.Day, 9, 25, 1, offset).AddDays(1);
        logger.LogInformation("SchedulerJob running next at: {time}.", nextStartDate);
        var scheduleTrigger = TriggerBuilder.Create()
            .StartAt(nextStartDate)
            .ForJob(schedulerJob)
            .Build();

        await scheduler.ScheduleJob(schedulerJob, scheduleTrigger);
    }
}