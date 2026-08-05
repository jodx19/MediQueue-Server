using MediatR;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.ValueObjects;
using MediQueue.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MediQueue.UnitTests.Infrastructure;

public class MultiTenantIsolationTests : IAsyncLifetime
{
    private static readonly Guid TenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private ClinicDbContext _context = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(
            options,
            new NullMediator(),
            new TestCurrentUserService(TenantA),
            new TestTenantContext(TenantA));

        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldOnlyReturnCurrentTenantPatients()
    {
        var patientA = CreatePatient(TenantA, "John", "Doe", "NAT-A-001");
        var patientB = CreatePatient(TenantB, "Jane", "Smith", "NAT-B-001");

        _context.Patients.AddRange(patientA, patientB);
        await _context.SaveChangesAsync();

        var patients = await _context.Patients.ToListAsync();

        Assert.Single(patients);
        Assert.Equal(TenantA, patients[0].TenantId);
        Assert.Equal("John", patients[0].PersonName.FirstName);
    }

    [Fact]
    public async Task IgnoreQueryFilters_WithTenantScope_ReturnsOnlyRequestedTenant()
    {
        var patientA = CreatePatient(TenantA, "Alice", "Brown", "NAT-A-002");
        var patientB = CreatePatient(TenantB, "Bob", "Green", "NAT-B-002");

        _context.Patients.AddRange(patientA, patientB);
        await _context.SaveChangesAsync();

        var patients = await _context.Patients
            .IgnoreQueryFilters()
            .Where(p => p.TenantId == TenantA && !p.IsDeleted)
            .ToListAsync();

        Assert.Single(patients);
        Assert.Equal(TenantA, patients[0].TenantId);
    }

    private static Patient CreatePatient(Guid tenantId, string firstName, string lastName, string nationalId)
    {
        var patient = Patient.Register(
            new PersonName(firstName, lastName),
            new DateOnly(1990, 1, 1),
            Gender.Male,
            BloodType.OPos,
            nationalId,
            new ContactInfo("01000000000"),
            new Address("1 Test St", "Cairo", "Cairo"),
            MaritalStatus.Single);

        patient.TenantId = tenantId;
        return patient;
    }

    private sealed class TestCurrentUserService : ICurrentUserService
    {
        public TestCurrentUserService(Guid tenantId) => TenantId = tenantId;

        public Guid? UserId => Guid.NewGuid();
        public Guid TenantId { get; }
        public string? Email => "test@mediqueue.com";
        public string? Role => "Admin";
        public Guid? PatientId => null;
        public Guid? DoctorId => null;
        public bool IsAuthenticated => true;
        public bool IsInRole(string role) => string.Equals(role, Role, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestTenantContext : ITenantContext
    {
        public TestTenantContext(Guid tenantId) => TenantId = tenantId;

        public Guid TenantId { get; }
        public string Subdomain { get; set; } = "test";
    }

    private sealed class NullMediator : IMediator
    {
        public Task Publish(object notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
            => Task.CompletedTask;

        public Task<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Not used in isolation tests.");

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
            where TRequest : IRequest
            => throw new NotSupportedException("Not used in isolation tests.");

        public Task<object?> Send(object request, CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Not used in isolation tests.");

        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
            IStreamRequest<TResponse> request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Not used in isolation tests.");

        public IAsyncEnumerable<object?> CreateStream(
            object request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException("Not used in isolation tests.");

        public Task<TResponse> Send<TRequest, TResponse>(
            TRequest request,
            CancellationToken cancellationToken = default)
            where TRequest : IRequest<TResponse>
            => throw new NotSupportedException("Not used in isolation tests.");
    }
}
