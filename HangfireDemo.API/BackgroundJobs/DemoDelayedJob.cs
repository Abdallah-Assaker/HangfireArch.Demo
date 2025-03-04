using Hangfire;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Configs.Attributes.Hangfire;
using HangfireDemo.API.Data.Abstract;
using MediatR;

namespace HangfireDemo.API.BackgroundJobs;

[JobConfiguration("Demo delayed job", 
    retryAttempts: 3, 
    queue: "io-bound", 
    retryDelaysInSeconds: [2], 
    onAttemptsExceeded: AttemptsExceededAction.Fail, 
    exceptOn: [typeof(NotImplementedException)])]
public record DemoDelayedJob(string Data) : IDelayedJob;

public class DemoDelayedJobHandler(IUnitOfWork uow, IMediator mediator) 
    : DelayedJobHandlerBase<DemoDelayedJob>
{
    protected override async Task Handle(DemoDelayedJob job)
    {
        Console.WriteLine($"DemoDelayedJob : Handle : Mediator: {mediator.GetHashCode()} UOW: {uow.GetHashCode()}");

        await Task.Delay(100);
        
        // Simulate error
        if (job.Data == "error")
            throw new InvalidOperationException("#DelayedJob: Demo error");
        
        if (job.Data == "not-implemented")
            throw new NotImplementedException("#DelayedJob: Demo not implemented");
    }
}