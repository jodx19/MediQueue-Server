// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Interfaces\IEmailService.cs
using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

public interface IEmailService
{
    Task SendPrescriptionAsync(string patientEmail, string prescriptionDetails);
    Task SendVisitSummaryAsync(string patientEmail, string visitSummary);
    Task SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Sends an email containing an email-verification link to the newly registered user.
    /// </summary>
    Task SendVerificationEmailAsync(string email, string userId, string verificationToken);
}
