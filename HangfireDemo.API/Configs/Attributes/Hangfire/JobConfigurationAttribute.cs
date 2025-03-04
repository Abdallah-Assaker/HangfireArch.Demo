using System.Reflection;
using Hangfire;

namespace HangfireDemo.API.Configs.Attributes.Hangfire;

[AttributeUsage(AttributeTargets.Class)]
public class JobConfigurationAttribute(
    string displayName,
    int retryAttempts = 3,
    AttemptsExceededAction onAttemptsExceeded = AttemptsExceededAction.Fail,
    int[]? retryDelaysInSeconds = null,
    Type[]? exceptOn = null,
    string queue = "default")
    : Attribute
{
    public string DisplayName { get; } = displayName;
    public int RetryAttempts { get; private set; } = retryAttempts;
    public int[] RetryDelaysInSeconds { get; private set; } = retryDelaysInSeconds ?? [1];
    public AttemptsExceededAction OnAttemptsExceeded { get; private set; } = onAttemptsExceeded;
    public Type[] ExceptOn { get; private set; } = exceptOn ?? [];
    public string Queue { get; private set; } = queue;
}