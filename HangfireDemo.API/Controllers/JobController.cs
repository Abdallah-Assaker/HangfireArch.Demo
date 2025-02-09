using HangfireDemo.API.BackgroundJobs;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Data.Abstract;
using HangfireDemo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HangfireDemo.API.Controllers;

[ApiController]
[Route("[controller]")]
public class JobController(IUnitOfWork uow, 
    IDelayedJobManager jobManager,
    IServiceProvider serviceProvider)
    : ControllerBase
{
    [HttpPost("delayed")]
    public IActionResult CreateDelayedJob(string data, int userId, string correlationId)
    {
        Console.WriteLine($"#Creating DelayedJob: uow in controller: {uow.GetHashCode()}");
    
        var context = new JobContext(
            userId: userId,
            correlationId: correlationId
        );
        
        context.Headers.Add("X-Request-Token", new Guid().ToString());
        
        jobManager.Schedule(
            new DemoDelayedJob(data),
            context,
            1500
        );

        return Ok(context.Headers["X-Request-Token"]);
    }
    
    [HttpPost("enqueue")]
    public IActionResult CreateEnqueuedJob(string data, int userId, string correlationId)
    {
        Console.WriteLine($"#Creating EnqueuedJob: uow in controller: {uow.GetHashCode()}");
    
        var context = new JobContext(
            userId: userId,
            correlationId: correlationId
        );
        
        context.Headers.Add("X-Request-Token", new Guid().ToString());
        
        jobManager.Enqueue(
            new DemoDelayedJob(data),
            context
        );

        return Ok(context.Headers["X-Request-Token"]);
    }
    
    [HttpGet("test")]
    public IActionResult Test()
    {
        Console.WriteLine($"#ServiceProvider: {serviceProvider.GetHashCode()}");
        return Ok();
    }
}