using System.Net;
using TimeTracker.Server.Business.Abstractions;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace TimeTracker.Server.Business.Services;

public class MailService : IMailService
{
    private readonly string _host;

    private readonly string _hostEmail;

    private readonly string _hostEmailPassword;

    private readonly int _hostPort;

    public MailService(IConfiguration configuration)
    {
        _host = configuration.GetSection("EmailSettings:Host").Value;
        _hostEmail = configuration.GetSection("EmailSettings:HostEmail").Value;
        _hostEmailPassword = configuration.GetSection("EmailSettings:Password").Value;
        _hostPort = int.Parse(configuration.GetSection("EmailSettings:Port").Value);
    }

    public async Task SendTextMessageAsync(string recipient, string subject, string text)
    {
        var smtpClient = new SmtpClient(_host, _hostPort)
        {
            UseDefaultCredentials = false,
            EnableSsl = true,
            Credentials = new NetworkCredential(_hostEmail, _hostEmailPassword)
        };

        var mailMessage = new MailMessage
        {
            From = new MailAddress(_hostEmail),
            Subject = subject,
            Body = text,
            IsBodyHtml = true
        };

        mailMessage.To.Add(recipient);

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception exception)
        {
            throw new Exception(exception.Message);
        }
    }
}