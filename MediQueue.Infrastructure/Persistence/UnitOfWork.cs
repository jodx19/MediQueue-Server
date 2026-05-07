using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;
using MediQueue.Infrastructure.Persistence.Repositories;

namespace MediQueue.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly ClinicDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    private IPatientRepository? _patientRepository;
    private IAppointmentRepository? _appointmentRepository;
    private IDoctorRepository? _doctorRepository;
    private IClinicalVisitRepository? _clinicalVisitRepository;
    private IInvoiceRepository? _invoiceRepository;
    private IUserRepository? _userRepository;
    private IMedicalAttachmentRepository? _attachmentRepository;
    private INotificationRepository? _notificationRepository;

    public UnitOfWork(ClinicDbContext context)
    {
        _context = context;
    }

    public IPatientRepository Patients => _patientRepository ??= new PatientRepository(_context);
    public IAppointmentRepository Appointments => _appointmentRepository ??= new AppointmentRepository(_context);
    public IDoctorRepository Doctors => _doctorRepository ??= new DoctorRepository(_context);
    public IClinicalVisitRepository ClinicalVisits => _clinicalVisitRepository ??= new ClinicalVisitRepository(_context);
    public IInvoiceRepository Invoices => _invoiceRepository ??= new InvoiceRepository(_context);
    public IUserRepository Users => _userRepository ??= new UserRepository(_context);
    public IMedicalAttachmentRepository Attachments => _attachmentRepository ??= new MedicalAttachmentRepository(_context);
    public INotificationRepository Notifications => _notificationRepository ??= new NotificationRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync()
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task ExecuteStrategyAsync(Func<Task> action)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync<object?, bool>(
            null,
            async (context, state, ct) =>
            {
                await action();
                return true;
            },
            null,
            default);
    }
}
