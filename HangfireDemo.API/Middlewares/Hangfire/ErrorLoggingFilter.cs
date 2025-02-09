using System.ComponentModel;
using System.Reflection;
using Hangfire.Client;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Helpers.Extensions;

namespace HangfireDemo.API.Middlewares.Hangfire;

public class ErrorLoggingFilter(ILogger<ErrorLoggingFilter> logger) 
    : IHangfireJobFilter
{
    public double ExecutionOrder => 1;
    public void OnCreating(CreatingContext context)
    {
    }

    public void OnCreated(CreatedContext context)
    {
    }

    public void OnPerforming(PerformingContext context)
    {
        Console.WriteLine($"ErrorLoggingFilter: OnPerforming : HangfireScope: {context.Items["HangfireScope"]?.GetHashCode()}, Context: {context.GetHashCode()}");
    }

    public void OnPerformed(PerformedContext context)
    {
        Console.WriteLine($"ErrorLoggingFilter : OnPerformed : HangfireScope: {context.Items["HangfireScope"]?.GetHashCode()}, Context: {context.GetHashCode()}");
        
        if (context.Exception is null) return;
        
        var job = context.BackgroundJob.Job;
        var contextData = context.GetAJobParameter<JobContext>();
             
        logger.LogError(context.Exception,
            "!!ErrorLoggingFilter!!: Job {JobType} execution failed. User: {UserId}, Correlation: {CorrelationId}, Args: {Args}",
            job.Type.GetCustomAttributes<DisplayNameAttribute>().FirstOrDefault()?.DisplayName ?? job.Type.Name,
            contextData?.UserId.ToString() ?? "unknown",
            contextData?.CorrelationId ?? "none",
            string.Join(", ", job.Args.Where(a => a is not JobContext)));
    }

    public void OnStateElection(ElectStateContext context)
    {
    }

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }
}