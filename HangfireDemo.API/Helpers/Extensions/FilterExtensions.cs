using Hangfire.Server;

namespace HangfireDemo.API.Helpers.Extensions;

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