using System.ComponentModel;
using Hangfire;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Helpers.Extensions;

namespace HangfireDemo.API.Services;

public class HangfireRecurrenceJobManager(
    IServiceProvider serviceProvider,
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
        var configuredDisplayName = job.GetJobConfigurationDisplayName();
        var configuredQueue = job.GetJobConfigurationQueue();
        
        // Use configured queue if the caller didn't specify one explicitly
        if (queue == "default" && configuredQueue != "default")
            queue = configuredQueue;
            
        recurringJobManager.AddOrUpdate(
            jobId,
            queue,
            () => ExecuteJob(job, context, configuredDisplayName),
            cron,
            new RecurringJobOptions 
            {
                TimeZone = TimeZoneInfo.Utc,
                MisfireHandling = MisfireHandlingMode.Relaxed
            }
        );
        logger.LogInformation("Updated recurring job {JobId} with schedule {Cron}", jobId, cron);
    }

    // The ExecuteJob method with the attributes applied dynamically at runtime
    [DisplayName("{2}")]
    public Task ExecuteJob<TJob>(TJob job, JobContext context, string jobName) where TJob : IRecurrenceJob
    {
        // The attributes are now applied to the job class itself, not this method
        var handler = serviceProvider.GetRequiredService<IRecurrenceJobHandlerBase<TJob>>();
        return handler.Execute(job, context);
    }
}