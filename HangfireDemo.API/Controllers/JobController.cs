using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Commands;
using HangfireDemo.API.Data.Abstract;
using HangfireDemo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HangfireDemo.API.Controllers;

[ApiController]
[Route("[controller]")]
public class JobController : ControllerBase
{
    private readonly IJobManager _jobManager;
    private readonly IUnitOfWork _uow;

    public JobController(IUnitOfWork uow, IJobManager jobManager)
    {
        _uow = uow;
        _jobManager = jobManager;
    }

    [HttpPost("delayed")]
    public IActionResult CreateDelayedJob(string data, int userId, string correlationId)
    {
        Console.WriteLine($"#Creating DelayedJob: uow in controller: {_uow.GetHashCode()}");
    
        var context = new JobContext(
            userId: userId,
            correlationId: correlationId
        );
        
        context.Headers.Add("X-Request-Token", new Guid().ToString());

        _jobManager.Schedule(
            new DelayedCommand(data), 
            context,
            TimeSpan.FromSeconds(5)
        );

        return Ok(new { context.CorrelationId });
    }
}