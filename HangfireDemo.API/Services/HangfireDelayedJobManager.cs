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

    [DisplayName("{2}"),
     AutomaticRetry(Attempts = 3, 
        DelaysInSeconds = [1],
        OnAttemptsExceeded = AttemptsExceededAction.Fail, //Failed jobs do not become expired to allow you to re-queue them without any time pressure. You should re-queue or delete them manually, or apply AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete) attribute to delete them automatically.
        LogEvents = true 
    // ,ExceptOn = Custom Business Exception
    )]
    // ReSharper disable once MemberCanBePrivate.Global
    public Task ExecuteJob<TJob>(TJob job, JobContext context, string jobName) where TJob : IDelayedJob
    {
        var handler = serviceProvider.GetRequiredService<IDelayedJobHandlerBase<TJob>>();
        return handler.Execute(job, context);

    }
}