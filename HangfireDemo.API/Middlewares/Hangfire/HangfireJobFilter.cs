using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using HangfireDemo.API.Data.Abstract;

namespace HangfireDemo.API.Middlewares.Hangfire;

public class HangfireJobFilter(IServiceScopeFactory scopeFactory)
    : JobFilterAttribute, IClientFilter, IServerFilter, IElectStateFilter, IApplyStateFilter 
{
    public void OnCreating(CreatingContext context)
    {
    }

    public void OnCreated(CreatedContext context)
    {
    }

    public void OnPerforming(PerformingContext context)
    {
        Console.WriteLine($"ServiceScopeFactory: {scopeFactory.GetHashCode()}");
        
        var scope = scopeFactory.CreateScope();
        
        context.Items["HangfireScope"] = scope;
        
        var ascendingSortedHangfireJobFilters = scope
            .ServiceProvider
            .GetServices<IHangfireJobFilter>()?
            .OrderBy(x => x.ExecutionOrder) ?? Enumerable.Empty<IHangfireJobFilter>();
        
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        uow.UserId = 159;
        
        foreach (var hangfireJobFilter in ascendingSortedHangfireJobFilters)
        {
            hangfireJobFilter.OnPerforming(context);
        }
        
        Console.WriteLine($"HangfireFilter : OnPerforming : HangfireScope: {scope.GetHashCode()}, Context: {context.GetHashCode()}, UOW: {uow.GetHashCode()}");
    }

    public void OnPerformed(PerformedContext context)
    {
        var scope = context.Items["HangfireScope"] as IServiceScope;
        
        var descendingSortedHangfireJobFilters = scope?
            .ServiceProvider
            .GetServices<IHangfireJobFilter>()?
            .OrderByDescending(x => x.ExecutionOrder) ?? Enumerable.Empty<IHangfireJobFilter>();

        foreach (var hangfireJobFilter in descendingSortedHangfireJobFilters)
        {
            hangfireJobFilter.OnPerformed(context);
        }
        
        Console.WriteLine($"HangfireFilter : OnPerformed : HangfireScope: {scope?.GetHashCode()}, Context: {context.GetHashCode()}");
        
        scope?.Dispose();
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