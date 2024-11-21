using Quartz;
using MarketViewer.Contracts.Enums;

namespace MarketViewer.Api.Jobs;

public static class ServiceCollectionExtensions
{
    public static List<(IJobDetail, ITrigger)> RegisterMarketDataJobs(this IServiceCollection services)
    {
        List<(IJobDetail, ITrigger)> jobs = [];

        var now = DateTimeOffset.Now;
        var minuteStartTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, now.AddMinutes(1).Minute, 1, 0, now.Offset);
        // Start at 9:01, 10:01, etc. to get the minute before: 9:00, 10:00, etc.
        var hourStartTime = new DateTimeOffset(now.Year, now.Month, now.Day, now.AddHours(1).Hour, 1, 1, 0, now.Offset);

        var initJob = JobBuilder.Create<InitializeJob>()
            .WithIdentity("ticker")
            .UsingJobData("date", DateTimeOffset.Now.ToString())
            .Build();

        var initTrigger = TriggerBuilder.Create()
            .WithIdentity("initTrigger")
            .StartNow()
            .ForJob(initJob)
            .Build();

        jobs.Add((initJob, initTrigger));

        var snapshotMinuteJob = JobBuilder.Create<SnapshotJob>()
            .WithIdentity("snapshotMinute")
            .UsingJobData("timespan", Timespan.minute.ToString())
            .Build();

        var snapshotMinuteTrigger = TriggerBuilder.Create()
            .WithIdentity("snapshotMinuteTrigger")
            .StartAt(minuteStartTime)
            .WithSimpleSchedule(schedule => schedule
                .WithIntervalInMinutes(1)
                .RepeatForever())
            .ForJob(snapshotMinuteJob)
            .Build();

        jobs.Add((snapshotMinuteJob, snapshotMinuteTrigger));

        var snapshotHourJob = JobBuilder.Create<SnapshotJob>()
            .WithIdentity("snapshotHour")
            .UsingJobData("timespan", Timespan.hour.ToString())
            .Build();

        var snapshotHourTrigger = TriggerBuilder.Create()
            .WithIdentity("snapshotHourTrigger")
            .StartAt(hourStartTime)
            .WithSimpleSchedule(schedule => schedule
                .WithIntervalInHours(1)
                .RepeatForever())
            .ForJob(snapshotHourJob)
            .Build();

        jobs.Add((snapshotHourJob, snapshotHourTrigger));

        return jobs;
    }
}
