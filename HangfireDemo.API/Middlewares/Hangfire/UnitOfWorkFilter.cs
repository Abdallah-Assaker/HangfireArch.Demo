using Hangfire.Common;
using Hangfire.Server;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Data.Abstract;
using HangfireDemo.API.Helpers.Extensions;

namespace HangfireDemo.API.Middlewares.Hangfire;

public class UnitOfWorkFilter : JobFilterAttribute, IServerFilter
{
    private readonly IServiceScopeFactory _scopeFactory;

    public UnitOfWorkFilter(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void OnPerforming(PerformingContext context)
    {
        var scope = _scopeFactory.CreateScope();
        context.Items["HangfireScope"] = scope;
        
        var jobContext = context.GetAJobParameter<JobContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        if (jobContext is not null)
            uow.UserId = jobContext.UserId;

        uow.Begin();
        
        Console.WriteLine($"&UnitOfWork: uow in UnitOfWorkFilter.OnPerforming: {uow.GetHashCode()}");
    }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Items.TryGetValue("HangfireScope", out var scopeObj) && scopeObj is IServiceScope scope)
        {
            try
            {
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                
                if (context.Exception == null)
                    uow.Commit();
                else
                    uow.Rollback();
            
                Console.WriteLine($"&UnitOfWork: uow in UnitOfWorkFilter.OnPerformed: {uow.GetHashCode()}");
            }
            finally
            {
                scope.Dispose();
                context.Items.Remove("HangfireScope");
            }
        }
    }
}