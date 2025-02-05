namespace HangfireDemo.API.BackgroundJobs.BuildingBlocks;

public abstract class BackgroundJobHandlerBase<TJob> : IBackgroundJobHandlerBase<TJob> 
    where TJob : IJob
{
    public async Task Execute(TJob job, JobContext context)
    {
        Console.WriteLine($"$BackgroundJob: Executing job for user {context.UserId}");
        await Handle(job);
    }

    protected abstract Task Handle(TJob job);
}

public abstract class DelayedJobHandlerBase<TJob> : BackgroundJobHandlerBase<TJob>, IDelayedJobHandlerBase<TJob>
    where TJob : IDelayedJob {}

public abstract class RecurrenceJobHandlerBase<TJob> : BackgroundJobHandlerBase<TJob>, IRecurrenceJobHandlerBase<TJob>
    where TJob : IRecurrenceJob {}