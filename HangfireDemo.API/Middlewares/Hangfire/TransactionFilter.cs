using Hangfire.Client;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using HangfireDemo.API.Data.Abstract;

namespace HangfireDemo.API.Middlewares.Hangfire;

public class TransactionFilter(IUnitOfWork unitOfWork) : IHangfireJobFilter
{
    public double ExecutionOrder => 2;
    public void OnCreating(CreatingContext context)
    {
    }

    public void OnCreated(CreatedContext context)
    {
    }

    public void OnPerforming(PerformingContext context)
    {
        Console.WriteLine($"TransactionFilter : OnPerforming : HangfireScope: {context.Items["HangfireScope"]?.GetHashCode()}, Context: {context.GetHashCode()}, UOW: {unitOfWork.GetHashCode()}");
        unitOfWork.Begin();
        Task.Delay(1000).GetAwaiter().GetResult();
    }

    public void OnPerformed(PerformedContext context)
    {
        Console.WriteLine($"TransactionFilter : OnPerformed : HangfireScope: {context.Items["HangfireScope"]?.GetHashCode()}, Context: {context.GetHashCode()}, UOW: {unitOfWork.GetHashCode()}");
        
        if (context.Exception is null)
        {
            unitOfWork.Commit();
            Task.Delay(1000).GetAwaiter().GetResult();
        }
        else
        {
            unitOfWork.Rollback();
            Task.Delay(1000).GetAwaiter().GetResult();
        }
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