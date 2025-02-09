using Hangfire.Client;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Helpers.Extensions;

namespace HangfireDemo.API.Middlewares.Hangfire;

public class UserStateInitializerFilter 
    : IHangfireJobFilter
{
    public double ExecutionOrder => 3;
    public void OnCreating(CreatingContext context)
    {
    }

    public void OnCreated(CreatedContext context)
    {
    }

    public void OnPerforming(PerformingContext context)
    {
        Console.WriteLine($"UserStateInitializerFilter : OnPerforming : HangfireScope: {context.Items["HangfireScope"]?.GetHashCode()}, Context: {context.GetHashCode()}");
        
        var contextData = context.GetAJobParameter<JobContext>();
        
        if (contextData is null) return;
        
        // Set the user state
        var userId = contextData.UserId;
    }

    public void OnPerformed(PerformedContext context)
    {
        Console.WriteLine($"UserStateInitializerFilter : OnPerformed : HangfireScope: {context.Items["HangfireScope"]?.GetHashCode()}, Context: {context.GetHashCode()}");
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