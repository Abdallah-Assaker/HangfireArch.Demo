using System.Reflection;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Configs.Attributes.Hangfire;

// Extension method to get job configuration from job type
namespace HangfireDemo.API.Helpers.Extensions;

public static class JobConfigurationExtensions
{
    public static string GetJobConfigurationQueue<TJob>(this TJob job) 
        where TJob : IJob
    {
        // Get job type
        var jobType = job.GetType();
        
        // Try to get the JobConfiguration attribute
        var jobConfig = jobType.GetCustomAttribute<JobConfigurationAttribute>();
        
        return jobConfig?.Queue ?? "default";
    }

    public static string GetJobConfigurationDisplayName<TJob>(this TJob job) 
        where TJob : IJob
    {
        // Get job type
        var jobType = job.GetType();
        
        // Try to get the JobConfiguration attribute
        var jobConfig = jobType.GetCustomAttribute<JobConfigurationAttribute>();
        
        return jobConfig?.DisplayName ?? jobType.Name;
    }
}