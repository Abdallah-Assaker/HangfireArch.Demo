using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using MediatR;

namespace HangfireDemo.API.Services;

public interface IJobManager
{
    void Schedule<TCommand>(TCommand command, JobContext context, TimeSpan delay) 
        where TCommand : IRequest;
    
    void AddOrUpdateRecurring<TCommand>(
        string jobId, 
        TCommand command, 
        JobContext context,
        string cron,
        string queue = "default"
    ) where TCommand : IRequest;
}