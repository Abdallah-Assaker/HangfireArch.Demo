using System.ComponentModel;
using System.Reflection;
using Hangfire;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;

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
        var jobName = GetJobName(job);
        
        recurringJobManager.AddOrUpdate(
            jobId,
            queue,
            () => ExecuteJob(job, context, jobName),
            cron,
            new RecurringJobOptions 
            {
                TimeZone = TimeZoneInfo.Utc,
                MisfireHandling = MisfireHandlingMode.Relaxed
            }
        );
        
        logger.LogInformation("Updated recurring job {JobId} with schedule {Cron}", jobId, cron);
    }
    
    private static string GetJobName<TJob>(TJob job) where TJob : IRecurrenceJob
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
    public Task ExecuteJob<TJob>(TJob job, JobContext context, string jobName) where TJob : IRecurrenceJob
    {
        var handler = serviceProvider.GetRequiredService<IRecurrenceJobHandlerBase<TJob>>();
        return handler.Execute(job, context);

    }
}
