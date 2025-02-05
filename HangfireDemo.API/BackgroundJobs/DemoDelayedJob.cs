using System.ComponentModel;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Data.Abstract;
using MediatR;

namespace HangfireDemo.API.BackgroundJobs;

[DisplayName("Demo delayed job")]
public record DemoDelayedJob(string Data) : IDelayedJob;

public class DemoDelayedJobHandler(IUnitOfWork uow, IMediator mediator) 
    : DelayedJobHandlerBase<DemoDelayedJob>
{
    protected override async Task Handle(DemoDelayedJob job)
    {
        Console.WriteLine($"#DelayedJob: Processing delayed job ({job.Data}) for user {uow.UserId}, [{uow.GetHashCode()}]");
        await Task.Delay(100);
        
        Console.WriteLine($"#DelayedJob: Mediator: {mediator.GetHashCode()}");
        
        // Simulate error
        if (job.Data == "error")
            throw new InvalidOperationException("#DelayedJob: Demo error");
    }
}