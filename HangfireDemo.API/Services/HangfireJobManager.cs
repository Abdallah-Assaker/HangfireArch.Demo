using HangfireDemo.API.BackgroundJobs.BuildingBlocks;

namespace HangfireDemo.API.Services;

public class HangfireJobManager(
    IDelayedJobManager delayedJobManager, 
    IRecurrenceJobManager recurrenceJobManager)
    : IJobManager
{
    public void Schedule<TJob>(TJob job, JobContext context, int delayedMilliseconds = 0, string queue = "default") 
        where TJob : IDelayedJob
    {
        delayedJobManager.Schedule(job, context, delayedMilliseconds, queue);
    }
    
    public void Enqueue<TJob>(TJob job, JobContext context, string queue = "default") 
        where TJob : IDelayedJob
    {
        delayedJobManager.Enqueue(job, context, queue);
    }
    
    public void AddOrUpdateRecurring<TJob>(
        string jobId, 
        TJob job, 
        JobContext context,
        string cron,
        string queue = "default"
    ) where TJob : IRecurrenceJob
    {
        recurrenceJobManager.AddOrUpdateRecurring(jobId, job, context, cron, queue);
    }
}