using System.ComponentModel;
using System.Reflection;
using Hangfire;
using Hangfire.Annotations;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;

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
        var jobName = GetJobName(job);
        
        backgroundJobManager.Schedule(
            queue,
            () => ExecuteJob(job, context, jobName),
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
        var jobName = GetJobName(job);
        
        backgroundJobManager.Enqueue(
            queue,
            () => ExecuteJob(job, context, jobName)
        );
        
        logger.LogInformation("Enqueued job {Id}", nameof(job));
    }
    
    private static string GetJobName<TJob>(TJob job) where TJob : IDelayedJob
    {
        var jobName = job.GetType().GetTypeInfo().GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? job.GetType().Name;
        return jobName;
    }

    [DisplayName("{2}")]
    // ReSharper disable once MemberCanBePrivate.Global
    public Task ExecuteJob<TJob>(TJob job, JobContext context, string jobName) where TJob : IDelayedJob
    {
        var handler = serviceProvider.GetRequiredService<IDelayedJobHandlerBase<TJob>>();
        return handler.Execute(job, context);

    }
}