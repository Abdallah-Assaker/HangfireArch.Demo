namespace HangfireDemo.API.BackgroundJobs.BuildingBlocks;

internal interface IBackgroundJobHandlerBase<in TJob> where TJob : IJob
{
    Task Execute(TJob job, JobContext context);
}

internal interface IDelayedJobHandlerBase<in TJob> : IBackgroundJobHandlerBase<TJob> where TJob : IDelayedJob { }

internal interface IRecurrenceJobHandlerBase<in TJob> : IBackgroundJobHandlerBase<TJob> where TJob : IRecurrenceJob { }
