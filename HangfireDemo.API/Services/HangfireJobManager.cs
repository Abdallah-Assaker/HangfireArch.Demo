using Hangfire;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using MediatR;

namespace HangfireDemo.API.Services;

public class HangfireJobManager : IJobManager
{
    private readonly IBackgroundJobClient _backgroundJob;
    private readonly IRecurringJobManager _recurringJobs;
    private readonly ILogger<HangfireJobManager> _logger;

    public HangfireJobManager(
        IBackgroundJobClient backgroundJob,
        IRecurringJobManager recurringJobs,
        ILogger<HangfireJobManager> logger)
    {
        _backgroundJob = backgroundJob;
        _recurringJobs = recurringJobs;
        _logger = logger;
    }

    public void Schedule<TCommand>(TCommand command, JobContext context, TimeSpan delay) 
        where TCommand : IRequest
    {
        _backgroundJob.Schedule<IBackgroundJob<TCommand>>(
            x => x.Execute(command, context),
            delay
        );

        _logger.LogInformation("Scheduled {CommandType} with correlation {CorrelationId}", typeof(TCommand).Name,
            context.CorrelationId);
    }

    public void AddOrUpdateRecurring<TCommand>(
        string jobId, 
        TCommand command, 
        JobContext context,
        string cron,
        string queue = "default"
    ) where TCommand : IRequest
    {
        _recurringJobs.AddOrUpdate<IBackgroundJob<TCommand>>(
            jobId,
            queue,
            x => x.Execute(command, context),
            cron,
            new RecurringJobOptions 
            {
                TimeZone = TimeZoneInfo.Utc,
                MisfireHandling = MisfireHandlingMode.Relaxed
            }
        );
        
        _logger.LogInformation("Updated recurring job {JobId} with schedule {Cron}", jobId, cron);
    }
}