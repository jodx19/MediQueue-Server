// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Interfaces\IUnitOfWork.cs
using System.Threading;
using System.Threading.Tasks;


namespace MediQueue.Domain.Interfaces;

public interface IUnitOfWork
{
    IPatientRepository Patients { get; }
    IAppointmentRepository Appointments { get; }
    IDoctorRepository Doctors { get; }
    IClinicalVisitRepository ClinicalVisits { get; }
    IInvoiceRepository Invoices { get; }
    IUserRepository Users { get; }
    IMedicalAttachmentRepository Attachments { get; }
    INotificationRepository Notifications { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
