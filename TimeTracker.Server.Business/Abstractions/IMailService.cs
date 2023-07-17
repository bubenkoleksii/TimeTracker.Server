namespace TimeTracker.Server.Business.Abstractions;

public interface IMailService
{
    public Task SendTextMessageAsync(string recipient, string subject, string text);
}