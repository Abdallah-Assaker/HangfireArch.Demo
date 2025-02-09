namespace HangfireDemo.API.Data.Abstract;

public interface IUnitOfWork
{
    int UserId { get; set; }
    void Begin();
    void Commit();
    void Rollback();
    void Publish();
}