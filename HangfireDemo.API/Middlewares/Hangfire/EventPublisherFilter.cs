using Hangfire.Client;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using HangfireDemo.API.Data.Abstract;

namespace HangfireDemo.API.Middlewares.Hangfire;

public class EventPublisherFilter(IUnitOfWork unitOfWork) : IHangfireJobFilter
{
    public double ExecutionOrder => 4;
    public void OnCreating(CreatingContext context)
    {
    }

    public void OnCreated(CreatedContext context)
    {
    }

    public void OnPerforming(PerformingContext context)
    {
        Console.WriteLine($"EventPublisherFilter : OnPerforming : HangfireScope: {context.Items["HangfireScope"]?.GetHashCode()}, Context: {context.GetHashCode()}, UOW: {unitOfWork.GetHashCode()}");
    }

    public void OnPerformed(PerformedContext context)
    {
        Console.WriteLine($"EventPublisherFilter : OnPerformed : HangfireScope: {context.Items["HangfireScope"]?.GetHashCode()}, Context: {context.GetHashCode()}, UOW: {unitOfWork.GetHashCode()}");
        
        if (context.Exception is null) return;
    
        unitOfWork.Rollback();
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