using Hangfire;
using HangfireDemo.API;
using HangfireDemo.API.BackgroundJobs;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Commands;
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
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("Hangfire")));

// Configure Hangfire Server
builder.Services.AddHangfireServer(options => {
    options.Queues = ["critical", "default"]; // Process these queues
    options.ServerName = "AssakerWorker"; // Name of the server    
});

builder.Services.AddHangfireServer(options => {
    options.Queues = new[] { "io-bound" };
    options.ServerName = "IO_Worker";
});

// Register services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IJobManager, HangfireJobManager>();
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

using (var scope = app.Services.CreateScope())
{
    var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
    
    jobManager.AddOrUpdateRecurring(
        "recurring-demo",
        new RecurringCommand(),
        new JobContext(999, "system"),
        "*/1 * * * *"
    );
}


app.Run();