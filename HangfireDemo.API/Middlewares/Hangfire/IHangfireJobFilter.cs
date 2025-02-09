using Hangfire.Client;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;

namespace HangfireDemo.API.Middlewares.Hangfire;

public interface IHangfireJobFilter
{
    public double ExecutionOrder { get; }
    
    public void OnCreating(CreatingContext context);

    public void OnCreated(CreatedContext context);

    public void OnPerforming(PerformingContext context);

    public void OnPerformed(PerformedContext context);

    public void OnStateElection(ElectStateContext context);

    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction);

    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction);
}