using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Configs.Attributes.Hangfire;
using HangfireDemo.API.Data.Abstract;
using MediatR;

namespace HangfireDemo.API.BackgroundJobs;

[JobConfiguration("Demo recurrence job", retryAttempts: 3)]
public record DemoRecurrenceJob(int Number) : IRecurrenceJob;

public class DemoRecurrenceJobHandler(IUnitOfWork uow, IMediator mediator) 
    : RecurrenceJobHandlerBase<DemoRecurrenceJob>
{
    protected override async Task Handle(DemoRecurrenceJob job)
    {
        Console.WriteLine($"DemoRecurrenceJob : Handle : Mediator: {mediator.GetHashCode()} UOW: {uow.GetHashCode()}");
        await Task.Delay(1500);
        
        var rnd = new Random();
        var error = rnd.Next(1, 10);
        
        if (error % 5 == 0 || error % 3 == 0)
            throw new InvalidOperationException($"@RecurringJob: Demo error: {error}");
    }
}