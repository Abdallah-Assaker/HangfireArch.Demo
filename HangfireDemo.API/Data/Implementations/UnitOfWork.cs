using HangfireDemo.API.Data.Abstract;

namespace HangfireDemo.API.Data.Implementations;

public class UnitOfWork : IUnitOfWork
{
    public int UserId { get; set; }
    
    public void Begin() => Console.WriteLine($"*UnitOfWork: [{GetHashCode()}] Beginning work for user {UserId}");
    public void Commit() => Console.WriteLine($"*UnitOfWork: [{GetHashCode()}] Committing work for user {UserId}");
    public void Rollback() => Console.WriteLine($"*UnitOfWork: [{GetHashCode()}] Rolling back work for user {UserId}");
    public void Publish() => Console.WriteLine($"*UnitOfWork: [{GetHashCode()}] Publishing events for user {UserId}");
}