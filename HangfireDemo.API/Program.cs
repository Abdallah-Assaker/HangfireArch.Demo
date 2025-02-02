using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using MediatR;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire")));

// Configure Hangfire Server
builder.Services.AddHangfireServer();

// Register services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBackgroundJob<RecurringCommand>, RecurringJobService>();
builder.Services.AddScoped<IBackgroundJob<DelayedCommand>, DelayedJobService>();

builder.Services.AddSingleton<ErrorLoggingFilter>();
builder.Services.AddSingleton<UnitOfWorkFilter>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    GlobalJobFilters.Filters.Add(
        serviceProvider.GetRequiredService<ErrorLoggingFilter>(),
        order: 0
    );
    
    GlobalJobFilters.Filters.Add(
        serviceProvider.GetRequiredService<UnitOfWorkFilter>(),
        order: 1
    );
}

// Middleware pipeline
app.UseHangfireDashboard();
app.MapControllers();

// Seed recurring job
RecurringJob.AddOrUpdate<IBackgroundJob<RecurringCommand>>(
    "recurring-demo",
    x => x.Execute(new RecurringCommand(), new JobContext(999, "system")),
    Cron.Minutely);

app.Run();

// UnitOfWork.cs
public interface IUnitOfWork
{
    int UserId { get; set; }
    void Begin();
    void Commit();
    void Rollback();
}

public class UnitOfWork : IUnitOfWork
{
    public int UserId { get; set; }
    
    public void Begin() => Console.WriteLine($"*UnitOfWork: [{GetHashCode()}] Beginning work for user {UserId}");
    public void Commit() => Console.WriteLine($"*UnitOfWork: [{GetHashCode()}] Committing work for user {UserId}");
    public void Rollback() => Console.WriteLine($"*UnitOfWork: [{GetHashCode()}] Rolling back work for user {UserId}");
}

// JobContext.cs
[Serializable]
public record JobContext(int UserId, string CorrelationId);

// Filters/UnitOfWorkFilter.cs
public class UnitOfWorkFilter : JobFilterAttribute, IServerFilter
{
    // private readonly IServiceScopeFactory _scopeFactory;
    //
    // public UnitOfWorkFilter(IServiceScopeFactory scopeFactory)
    // {
    //     _scopeFactory = scopeFactory;
    // }
    
    private readonly IUnitOfWork _uow;

    public UnitOfWorkFilter(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public void OnPerforming(PerformingContext context)
    {
        // var scope = _scopeFactory.CreateScope();
        // context.Items["HangfireScope"] = scope;
        
        var jobContext = context.GetAJobParameter<JobContext>();
        // var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        if (jobContext is not null)
            _uow.UserId = jobContext.UserId;

        _uow.Begin();
        
        Console.WriteLine($"&UnitOfWork: uow in UnitOfWorkFilter.OnPerforming: {_uow.GetHashCode()}");
    }

    public void OnPerformed(PerformedContext context)
    {
        // if (context.Items.TryGetValue("HangfireScope", out var scopeObj) && scopeObj is IServiceScope scope)
        // {
            // var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            
            var uow = _uow;
            
            if (context.Exception == null)
                uow.Commit();
            else
                uow.Rollback();

            // scope.Dispose();

            Console.WriteLine($"&UnitOfWork: uow in UnitOfWorkFilter.OnPerformed: {uow.GetHashCode()}");
        // }
    }
}

// Filters/ErrorLoggingFilter.cs
public class ErrorLoggingFilter : JobFilterAttribute, IServerFilter
{
    private readonly ILogger<ErrorLoggingFilter> _logger;

    public ErrorLoggingFilter(ILogger<ErrorLoggingFilter> logger)
    {
        _logger = logger;
    }

    public void OnPerforming(PerformingContext context) { }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Exception != null)
        {
            var job = context.BackgroundJob.Job;
            var contextData = context.GetAJobParameter<JobContext>();
            
            _logger.LogError(context.Exception,
                "!!ErrorLogging: Job {JobType} failed. User: {UserId}, Correlation: {CorrelationId}, Args: {Args}",
                job.Type.Name,
                contextData?.UserId.ToString() ?? "unknown",
                contextData?.CorrelationId ?? "none",
                string.Join(", ", job.Args.Where(a => a is not JobContext)));
        }
    }
}

public static class FilterExtensions
{
    public static T? GetAJobParameter<T>(this PerformingContext context) where T : class
    {
        return context.BackgroundJob.Job.Args
            .FirstOrDefault(arg => arg is T) as T;
    }
    
    public static T? GetAJobParameter<T>(this PerformedContext context) where T : class
    {
        return context.BackgroundJob.Job.Args
            .FirstOrDefault(arg => arg is T) as T;
    }
}

// Jobs/BackgroundJobBase.cs
public interface IBackgroundJob<TCommand> where TCommand : IRequest
{
    Task Execute(TCommand command, JobContext context);
}

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

// Jobs/RecurringJobService.cs
public record RecurringCommand : IRequest;

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

// Jobs/DelayedJobService.cs
public record DelayedCommand(string Data) : IRequest;

public class DelayedJobService : BackgroundJobBase<DelayedCommand>
{
    private readonly IUnitOfWork _uow;

    public DelayedJobService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    protected override async Task Handle(DelayedCommand command)
    {
        Console.WriteLine($"uow in DelayedCommand.Handle(): {_uow.GetHashCode()}");
        
        Console.WriteLine($"#DelayedJob: Processing delayed job ({command.Data}) for user {_uow.UserId}, [{_uow.GetHashCode()}]");
        await Task.Delay(100);
        
        // Simulate error
        if (command.Data == "error")
            throw new InvalidOperationException("#DelayedJob: Demo error");
    }
}

// Controllers/JobController.cs
[ApiController]
[Route("[controller]")]
public class JobController : ControllerBase
{
    private readonly IBackgroundJobClient _backgroundJob;
    private readonly IUnitOfWork _uow;

    public JobController(
        IBackgroundJobClient backgroundJob,
        IUnitOfWork uow)
    {
        _backgroundJob = backgroundJob;
        _uow = uow;
    }

    [HttpPost("delayed")]
    public IActionResult CreateDelayedJob(string data, int userId, string correlationId)
    {
        Console.WriteLine($"#Creating DelayedJob: uow in controller: {_uow.GetHashCode()}");
        
        var context = new JobContext(
            UserId: userId,
            CorrelationId: correlationId
        );

        _backgroundJob.Schedule<IBackgroundJob<DelayedCommand>>(
            x => x.Execute(new DelayedCommand(data), context),
            TimeSpan.FromSeconds(5));

        return Ok(new { context.CorrelationId });
    }
}