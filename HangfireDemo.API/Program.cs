using Hangfire;
using HangfireDemo.API.BackgroundJobs;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Configs.Hangfire;
using HangfireDemo.API.Data.Abstract;
using HangfireDemo.API.Data.Implementations;
using HangfireDemo.API.Middlewares.Hangfire;
using HangfireDemo.API.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddSingleton<JobActivator, ContextAwareJobActivator>();

// Add Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSerializerSettings(new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.Auto,
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    })
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire"))
    .UseFilter(new AutomaticRetryAttribute { Attempts = 0 })
    .UseColouredConsoleLogProvider());

// Configure Hangfire Server
builder.Services.AddHangfireServer(options => {
    options.Queues = ["critical", "default"]; // Process these queues
    options.ServerName = "AssakerWorker"; // Name of the server    
});

builder.Services.AddHangfireServer(options => {
    options.Queues = ["io-bound"];
    options.ServerName = "IO_Worker";
});

// Register services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IJobManager, HangfireJobManager>();
builder.Services.AddScoped<IRecurrenceJobManager, HangfireRecurrenceJobManager>();
builder.Services.AddScoped<HangfireRecurrenceJobManager>();
builder.Services.AddScoped<IDelayedJobManager, HangfireDelayedJobManager>();
builder.Services.AddScoped<HangfireDelayedJobManager>();

builder.Services.AddScoped<IHangfireJobFilter, ErrorLoggingFilter>();
builder.Services.AddScoped<IHangfireJobFilter, UserStateInitializerFilter>();
builder.Services.AddScoped<IHangfireJobFilter, TransactionFilter>();
builder.Services.AddScoped<IHangfireJobFilter, EventPublisherFilter>();

builder.Services.AddScoped<IRecurrenceJobHandlerBase<DemoRecurrenceJob>, DemoRecurrenceJobHandler>();
builder.Services.AddScoped<IDelayedJobHandlerBase<DemoDelayedJob>, DemoDelayedJobHandler>();

builder.Services.AddSingleton<JobConfigurationFilter>();
builder.Services.AddSingleton<HangfireJobFilter>();

var app = builder.Build();

GlobalJobFilters.Filters.Add(
    app.Services.GetRequiredService<JobConfigurationFilter>(),
    order: -10 // Ensure it runs before other filters
);

GlobalJobFilters.Filters.Add(
    app.Services.GetRequiredService<HangfireJobFilter>(),
    order: 0
);

// Middleware pipeline
app.UseHangfireDashboard();
app.MapControllers();

// Seed recurring job
AddOrUpdateRecurringJobs(app);

app.Run();

void AddOrUpdateRecurringJobs(WebApplication webApplication)
{
    using var scope = webApplication.Services.CreateScope();
    var jobManager = scope.ServiceProvider.GetRequiredService<IRecurrenceJobManager>();
    
    jobManager.AddOrUpdateRecurring(
        "recurring-demo",
        new DemoRecurrenceJob(new Random().Next(1, 100)),
        new JobContext(999, "system"),
        "*/20 * * * * *"
    );
}