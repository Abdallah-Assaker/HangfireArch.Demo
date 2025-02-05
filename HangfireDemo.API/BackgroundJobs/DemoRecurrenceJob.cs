using System.ComponentModel;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Data.Abstract;
using MediatR;

namespace HangfireDemo.API.BackgroundJobs;

[DisplayName("Demo recurrence job")]
public record DemoRecurrenceJob(int Number) : IRecurrenceJob;

public class DemoRecurrenceJobHandler(IUnitOfWork uow, IMediator mediator) 
    : RecurrenceJobHandlerBase<DemoRecurrenceJob>
{
    protected override async Task Handle(DemoRecurrenceJob job)
    {
        Console.WriteLine($"@RecurringJob: Processing recurring job ({job.Number}) for user {uow.UserId}, [{uow.GetHashCode()}]");
        await Task.Delay(100);
        
        Console.WriteLine($"@RecurringJob: Mediator: {mediator.GetHashCode()}");
        
        // Simulate error
        var rnd = new Random();
        var error = rnd.Next(1, 10);
        
        if (error % 5 == 0)
            throw new InvalidOperationException($"@RecurringJob: Demo error: {error}");
    }
}