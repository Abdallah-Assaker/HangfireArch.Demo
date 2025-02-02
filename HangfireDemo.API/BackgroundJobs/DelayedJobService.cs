using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Commands;
using HangfireDemo.API.Data.Abstract;

namespace HangfireDemo.API.BackgroundJobs;

public class DelayedJobService : BackgroundJobBase<DelayedCommand>
{
    private readonly IUnitOfWork _uow;

    public DelayedJobService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    protected override async Task Handle(DelayedCommand command)
    {
        Console.WriteLine($"#DelayedJob: Processing delayed job ({command.Data}) for user {_uow.UserId}, [{_uow.GetHashCode()}]");
        await Task.Delay(100);
        
        // Simulate error
        if (command.Data == "error")
            throw new InvalidOperationException("#DelayedJob: Demo error");
    }
}