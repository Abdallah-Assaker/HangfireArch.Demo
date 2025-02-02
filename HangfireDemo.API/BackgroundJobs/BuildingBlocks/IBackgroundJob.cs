using MediatR;

namespace HangfireDemo.API.BackgroundJobs.BuildingBlocks;

public interface IBackgroundJob<TCommand> where TCommand : IRequest
{
    Task Execute(TCommand command, JobContext context);
}