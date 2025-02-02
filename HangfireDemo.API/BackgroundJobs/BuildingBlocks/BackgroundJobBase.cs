using MediatR;

namespace HangfireDemo.API.BackgroundJobs.BuildingBlocks;

public abstract class BackgroundJobBase<TCommand> : IBackgroundJob<TCommand> 
    where TCommand : IRequest
{
    public async Task Execute(TCommand command, JobContext context)
    {
        Console.WriteLine($"$BackgroundJob: Executing job for user {context.UserId}");
        await Handle(command);
    }

    protected abstract Task Handle(TCommand command);
}