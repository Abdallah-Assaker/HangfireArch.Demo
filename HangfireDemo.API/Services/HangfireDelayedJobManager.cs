using System.ComponentModel;
using Hangfire;
using Hangfire.Annotations;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Helpers.Extensions;

namespace HangfireDemo.API.Services;

public class HangfireDelayedJobManager(
    IServiceProvider serviceProvider,
    IBackgroundJobClient backgroundJobManager,
    ILogger<HangfireDelayedJobManager> logger)
    : IDelayedJobManager
{
    public void Schedule<TJob>(
        [NotNull] TJob job, 
        JobContext context, 
        int delayedMilliseconds = 0, 
        string queue = "default"
    ) where TJob : IDelayedJob
    {
        var configuredDisplayName = job.GetJobConfigurationDisplayName();
        var configuredQueue = job.GetJobConfigurationQueue();
        
        // Use configured queue if the caller didn't specify one explicitly
        if (queue == "default" && configuredQueue != "default")
            queue = configuredQueue;
            
        backgroundJobManager.Schedule(
            queue,
            () => ExecuteJob(job, context, configuredDisplayName),
            TimeSpan.FromMilliseconds(delayedMilliseconds)
        );
        logger.LogInformation("Scheduled job {Id} with delay {Delay}", nameof(job), delayedMilliseconds);
    }

    public void Enqueue<TJob>(
        [NotNull] TJob job, 
        JobContext context, 
        string queue = "default"
    ) where TJob : IDelayedJob
    {
        var configuredDisplayName = job.GetJobConfigurationDisplayName();
        var configuredQueue = job.GetJobConfigurationQueue();
        
        // Use configured queue if the caller didn't specify one explicitly
        if (queue == "default" && configuredQueue != "default")
            queue = configuredQueue;
            
        backgroundJobManager.Enqueue(
            queue,
            () => ExecuteJob(job, context, configuredDisplayName)
        );
        logger.LogInformation("Enqueued job {Id}", nameof(job));
    }

    // The ExecuteJob method without hardcoded attributes
    [DisplayName("{2}")]
    public Task ExecuteJob<TJob>(TJob job, JobContext context, string jobName) where TJob : IDelayedJob
    {
        // The attributes are now applied to the job class itself, not this method
        var handler = serviceProvider.GetRequiredService<IDelayedJobHandlerBase<TJob>>();
        return handler.Execute(job, context);
    }
}