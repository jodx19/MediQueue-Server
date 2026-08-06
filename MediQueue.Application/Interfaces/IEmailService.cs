// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Interfaces\IEmailService.cs
using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendPrescriptionAsync(string patientEmail, string prescriptionDetails);
    Task SendVisitSummaryAsync(string patientEmail, string visitSummary);
    Task SendVerificationEmailAsync(string toEmail, string verificationLink);
}
