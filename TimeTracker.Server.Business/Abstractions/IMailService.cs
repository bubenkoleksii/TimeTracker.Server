namespace TimeTracker.Server.Business.Abstractions;

public interface IMailService
{
    public Task SendTextMessage(string recipient, string subject, string text);
}