namespace HangfireDemo.API.BackgroundJobs.BuildingBlocks;

public interface IJob { }
public interface IDelayedJob : IJob { }
public interface IRecurrenceJob : IJob { }