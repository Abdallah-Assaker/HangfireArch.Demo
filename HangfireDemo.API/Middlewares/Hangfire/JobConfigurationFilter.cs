using Hangfire.Client;
using Hangfire.Server;
using Hangfire.States;
using System.Reflection;
using Hangfire;
using HangfireDemo.API.BackgroundJobs.BuildingBlocks;
using HangfireDemo.API.Configs.Attributes.Hangfire;
using HangfireDemo.API.Helpers.Extensions;

namespace HangfireDemo.API.Middlewares.Hangfire;

public class JobConfigurationFilter : IClientFilter, IServerFilter, IElectStateFilter
{
    public void OnCreating(CreatingContext context)
    {
        // Get the job instance (if available)
        var jobArg = context.Job.Args.FirstOrDefault(arg => arg is IJob);
        if (jobArg is not IJob job) return;
        
        // Get the job configuration
        var jobConfig = job.GetType().GetCustomAttribute<JobConfigurationAttribute>();
        if (jobConfig == null) return;
        
        context.SetJobParameter("DisplayName", job.GetJobConfigurationDisplayName());
        
        context.SetJobParameter("RetryAttempts", jobConfig.RetryAttempts);
        context.SetJobParameter("RetryDelaysInSeconds", jobConfig.RetryDelaysInSeconds);
        context.SetJobParameter("OnAttemptsExceeded", jobConfig.OnAttemptsExceeded);
        
        var exceptOnTypeNames = jobConfig.ExceptOn.Select(t => t.FullName).ToArray();
        context.SetJobParameter("ExceptOn", exceptOnTypeNames);
    }

    public void OnCreated(CreatedContext filterContext) { }

    public void OnPerforming(PerformingContext context) { }

    public void OnPerformed(PerformedContext context) { }

    public void OnStateElection(ElectStateContext context)
    {
        // If the job failed, check if we should retry based on our configuration
        if (context.CandidateState is FailedState failedState)
        {
            var exceptOn = context.GetJobParameter<string[]>("ExceptOn") ?? Array.Empty<string>();

            // Check if exception type is in ExceptOn
            var exceptionTypeName = failedState.Exception.GetType().FullName;
            if (exceptOn.Contains(exceptionTypeName))
            {
                // Do not retry for this exception type
                context.CandidateState = new FailedState(failedState.Exception)
                {
                    Reason = "Exception type is in ExceptOn list"
                };
                return;
            }
            
            // Get retry configuration
            var retryAttempts = context.GetJobParameter<int?>("RetryAttempts") ?? 3;
            
            var retryDelays = context.GetJobParameter<int[]>("RetryDelaysInSeconds") ?? [1];
            
            var exceededAction = context.GetJobParameter<AttemptsExceededAction?>("OnAttemptsExceeded") ?? AttemptsExceededAction.Fail;
            
            // Get current retry attempt count
            var currentAttempt = context.GetJobParameter<int?>("RetryCount") ?? 0;
            
            // Check if we should retry
            if (currentAttempt < retryAttempts)
            {
                // Determine delay for this retry
                var delayInSeconds = retryDelays.Length > currentAttempt 
                    ? retryDelays[currentAttempt] 
                    : retryDelays.LastOrDefault();
                
                // Create scheduled state for retry
                var scheduledState = new ScheduledState(TimeSpan.FromSeconds(delayInSeconds))
                {
                    Reason = $"Retry attempt {currentAttempt + 1} of {retryAttempts}"
                };
                
                // Update retry count
                context.SetJobParameter("RetryCount", currentAttempt + 1);
                
                // Set candidate state to scheduled for retry
                context.CandidateState = scheduledState;
            }
            else if (exceededAction == AttemptsExceededAction.Delete)
            {
                // If we've exceeded retry attempts and action is Delete, delete the job
                context.CandidateState = new DeletedState
                {
                    Reason = "Retry attempts exceeded"
                };
            }
        }
    }
}
