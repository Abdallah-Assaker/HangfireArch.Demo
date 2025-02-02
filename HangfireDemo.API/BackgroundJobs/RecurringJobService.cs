using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Commands;
using HangfireDemo.API.Data.Abstract;

namespace HangfireDemo.API.BackgroundJobs;

public class RecurringJobService : BackgroundJobBase<RecurringCommand>
{
    private readonly IUnitOfWork _uow;

    public RecurringJobService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    protected override async Task Handle(RecurringCommand command)
    {
        Console.WriteLine($"@RecurringJob: Processing recurring job for user {_uow.UserId}, [{_uow.GetHashCode()}]");
        await Task.Delay(100);
    }
}