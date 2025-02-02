using Hangfire.Common;
using Hangfire.Server;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Helpers.Extensions;

namespace HangfireDemo.API.Middlewares.Hangfire;

public class ErrorLoggingFilter : JobFilterAttribute, IServerFilter
{
    private readonly ILogger<ErrorLoggingFilter> _logger;

    public ErrorLoggingFilter(ILogger<ErrorLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnPerforming(PerformingContext context) { }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Exception != null)
        {
            var job = context.BackgroundJob.Job;
            var contextData = context.GetAJobParameter<JobContext>();
            
            _logger.LogError(context.Exception,
                "!!ErrorLogging: Job {JobType} failed. User: {UserId}, Correlation: {CorrelationId}, Args: {Args}",
                job.Type.Name,
                contextData?.UserId.ToString() ?? "unknown",
                contextData?.CorrelationId ?? "none",
                string.Join(", ", job.Args.Where(a => a is not JobContext)));
        }
    }
}