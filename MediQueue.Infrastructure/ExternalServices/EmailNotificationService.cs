using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using MediQueue.Application.Interfaces;

namespace MediQueue.Infrastructure.ExternalServices;

public class EmailNotificationService : IEmailService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IConfiguration _configuration;

    public EmailNotificationService(ILogger<EmailNotificationService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration["EmailSettings:FromEmail"] ?? "noreply@mediqueue.com"));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            var host = _configuration["EmailSettings:Host"] ?? "smtp.test.com";
            var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
            var secureSocketOptions = SecureSocketOptions.StartTls;

            await smtp.ConnectAsync(host, port, secureSocketOptions);
            
            var user = _configuration["EmailSettings:User"];
            var pass = _configuration["EmailSettings:Password"];
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                await smtp.AuthenticateAsync(user, pass);
            }

            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
        }
    }

    public async Task SendPrescriptionAsync(string patientEmail, string prescriptionDetails)
    {
        var htmlBody = $"<h1>Your Prescription</h1><p>{prescriptionDetails}</p>";
        await SendEmailAsync(patientEmail, "Your Prescription", htmlBody);
    }

    public async Task SendVisitSummaryAsync(string patientEmail, string visitSummary)
    {
        var htmlBody = $"<h1>Visit Summary</h1><p>{visitSummary}</p>";
        await SendEmailAsync(patientEmail, "Visit Summary", htmlBody);
    }
}
