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

/// <summary>
/// Production email service using MailKit/SMTP.
/// Configure via appsettings: EmailSettings:{ Host, Port, User, Password, FromEmail }.
/// Appointment confirmation/reminder emails are sent from AppointmentEventHandlers
/// using real patient email from the database.
/// </summary>
public class EmailNotificationService : IEmailService
{
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IConfiguration _configuration;

    public EmailNotificationService(
        ILogger<EmailNotificationService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(
                _configuration["EmailSettings:FromEmail"] ?? "noreply@mediqueue.com"));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

            using var smtp = new SmtpClient();
            var host = _configuration["EmailSettings:Host"] ?? "smtp.test.com";
            var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");

            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);

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
        await SendEmailAsync(patientEmail, "Your Prescription — MediQueue", htmlBody);
    }

    public async Task SendVisitSummaryAsync(string patientEmail, string visitSummary)
    {
        var htmlBody = $"<h1>Visit Summary</h1><p>{visitSummary}</p>";
        await SendEmailAsync(patientEmail, "Visit Summary — MediQueue", htmlBody);
    }

    public async Task SendVerificationEmailAsync(string toEmail, string verificationLink)
    {
        var htmlBody = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:32px;background:#f9fafb;border-radius:12px;">
              <h1 style="color:#0f766e;margin-bottom:8px;">Verify Your Email Address</h1>
              <p style="color:#374151;font-size:15px;">Thank you for registering with MediQueue. Please click the button below to verify your email address.</p>
              <div style="text-align:center;margin:32px 0;">
                <a href="{verificationLink}"
                   style="background:#0f766e;color:#ffffff;padding:14px 32px;border-radius:8px;text-decoration:none;font-weight:bold;font-size:16px;display:inline-block;">
                  Verify Email Address
                </a>
              </div>
              <p style="color:#6b7280;font-size:13px;">This link expires in 24 hours. If you did not create an account, you can safely ignore this email.</p>
              <hr style="border:none;border-top:1px solid #e5e7eb;margin:24px 0;"/>
              <p style="color:#9ca3af;font-size:12px;">MediQueue EMR · Clinic Management System</p>
            </div>
            """;
        await SendEmailAsync(toEmail, "Verify Your Email — MediQueue", htmlBody);
    }
}
