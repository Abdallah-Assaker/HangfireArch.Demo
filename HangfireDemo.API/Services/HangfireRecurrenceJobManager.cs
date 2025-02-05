using Hangfire;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;

namespace HangfireDemo.API.Services;

public class HangfireRecurrenceJobManager(
    IRecurringJobManager recurringJobManager,
    ILogger<HangfireRecurrenceJobManager> logger)
    : IRecurrenceJobManager
{
    public void AddOrUpdateRecurring<TJob>(
        string jobId, 
        TJob job, 
        JobContext context,
        string cron,
        string queue = "default"
    ) where TJob : IRecurrenceJob
    {
        recurringJobManager.AddOrUpdate<IRecurrenceJobHandlerBase<TJob>>(
            jobId,
            queue,
            x => x.Execute(job, context),
            cron,
            new RecurringJobOptions 
            {
                TimeZone = TimeZoneInfo.Utc,
                MisfireHandling = MisfireHandlingMode.Relaxed
            }
        );
        
        logger.LogInformation("Updated recurring job {JobId} with schedule {Cron}", jobId, cron);
    }
}

public class HangfireDelayedJobManager(
    IBackgroundJobClient backgroundJobManager,
    ILogger<HangfireDelayedJobManager> logger)
    : IDelayedJobManager
{
    public void Schedule<TJob>(
        TJob job, 
        JobContext context, 
        int delayedMilliseconds = 0, 
        string queue = "default"
    ) where TJob : IDelayedJob
    {
        backgroundJobManager.Schedule<IDelayedJobHandlerBase<TJob>>(
            queue,
            x => x.Execute(job, context),
            TimeSpan.FromMilliseconds(delayedMilliseconds)
        );
        
        logger.LogInformation("Scheduled job {Id} with delay {Delay}", nameof(job), delayedMilliseconds);
    }

    public void Enqueue<TJob>(
        TJob job, 
        JobContext context, 
        string queue = "default"
    ) where TJob : IDelayedJob
    {
        backgroundJobManager.Enqueue<IDelayedJobHandlerBase<TJob>>(
            queue,
            x => x.Execute(job, context)
        );
        
        logger.LogInformation("Enqueued job {Id}", nameof(job));
    }
}