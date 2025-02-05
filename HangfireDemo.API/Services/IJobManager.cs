using HangfireDemo.API.BackgroundJobs.BuildingBlocks;

namespace HangfireDemo.API.Services;

public interface IJobManager : IRecurrenceJobManager, IDelayedJobManager { }


public interface IRecurrenceJobManager
{
    /// <summary>
    /// Adds or updates a recurring job to be enqueued to the specified queue.
    /// </summary>
    ///
    /// <typeparam name="TJob">Type whose handle method will be invoked during job processing.</typeparam>
    /// <param name="jobId">Unique identifier for the job.</param>
    /// <param name="job">Job to be enqueued.</param>
    /// <param name="context">Context for the background job.</param>
    /// <param name="cron">Cron expression for the job to be enqueued.</param>
    /// <param name="queue">Default queue for the background job.</param>
    void AddOrUpdateRecurring<TJob>(
        string jobId, 
        TJob job, 
        JobContext context,
        string cron,
        string queue = "default"
    ) where TJob : IRecurrenceJob;
}

public interface IDelayedJobManager
{
    /// <summary>
    /// Schedules a background job and schedules it to be enqueued to the specified queue after a given delay.
    /// </summary>
    ///
    /// <typeparam name="TJob">Type whose handle method will be invoked during job processing.</typeparam>
    /// <param name="job">Job to be enqueued.</param>
    /// <param name="context">Context for the background job.</param>
    /// <param name="delayedMilliseconds">Delay in milliseconds before the job is enqueued.</param>
    /// <param name="queue">Default queue for the background job.</param>
    void Schedule<TJob>(TJob job, JobContext context, int delayedMilliseconds = 0, string queue = "default") 
        where TJob : IDelayedJob;

    /// <summary>
    /// Creates a background job and places it into the specified queue.
    /// </summary>
    ///
    /// <typeparam name="TJob">Type whose handle method will be invoked during job processing.</typeparam>
    /// <param name="job">Job to be enqueued.</param>
    /// <param name="context">Context for the background job.</param>
    /// <param name="queue">Default queue for the background job.</param>
    void Enqueue<TJob>(TJob job, JobContext context, string queue = "default")    
        where TJob : IDelayedJob;
}