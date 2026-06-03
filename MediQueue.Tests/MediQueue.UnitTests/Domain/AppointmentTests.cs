using FluentAssertions;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Events;
using MediQueue.Domain.Exceptions;
using MediQueue.UnitTests.Domain.Builders;

namespace MediQueue.UnitTests.Domain;

public class AppointmentTests
{
    [Fact]
    public void Book_WithValidData_ShouldCreateAppointmentWithScheduledStatus()
    {
        var appointment = new AppointmentTestBuilder().Build();

        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.PatientId.Should().NotBeEmpty();
        appointment.DoctorId.Should().NotBeEmpty();
        appointment.ChiefComplaint.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Book_ShouldRaiseAppointmentBookedEvent()
    {
        var appointment = new AppointmentTestBuilder().Build();

        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentBookedEvent);
    }

    [Fact]
    public void Confirm_WhenScheduled_ShouldChangeStatusToConfirmed()
    {
        var appointment = new AppointmentTestBuilder().Build();

        appointment.Confirm();

        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void Confirm_ShouldRaiseAppointmentConfirmedEvent()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.ClearDomainEvents();

        appointment.Confirm();

        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentConfirmedEvent);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_ShouldThrowInvalidAppointmentStatusException()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Confirm();

        var act = () => appointment.Confirm();

        act.Should().Throw<InvalidAppointmentStatusException>();
    }

    [Fact]
    public void Cancel_WithReason_ShouldChangeStatusToCancelled()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Confirm();

        appointment.Cancel("Patient requested cancellation");

        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancellationReason.Should().Be("Patient requested cancellation");
    }

    [Fact]
    public void Cancel_ShouldRaiseAppointmentCancelledEvent()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Confirm();
        appointment.ClearDomainEvents();

        appointment.Cancel("Scheduling conflict");

        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentCancelledEvent);
        var evt = appointment.DomainEvents.OfType<AppointmentCancelledEvent>().First();
        evt.Reason.Should().Be("Scheduling conflict");
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrowInvalidAppointmentStatusException()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Confirm();
        appointment.CheckIn();
        appointment.Start();
        appointment.Complete();

        var act = () => appointment.Cancel("Too late");

        act.Should().Throw<InvalidAppointmentStatusException>();
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowInvalidAppointmentStatusException()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Cancel("Changed mind");

        var act = () => appointment.Cancel("Double cancel");

        act.Should().Throw<InvalidAppointmentStatusException>();
    }

    [Fact]
    public void CheckIn_WhenConfirmed_ShouldChangeStatusToCheckedIn()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Confirm();

        appointment.CheckIn();

        appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
        appointment.ActualStartTime.Should().NotBeNull();
    }

    [Fact]
    public void Start_WhenCheckedIn_ShouldChangeStatusToInProgress()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Confirm();
        appointment.CheckIn();

        appointment.Start();

        appointment.Status.Should().Be(AppointmentStatus.InProgress);
    }

    [Fact]
    public void Start_ShouldRaiseAppointmentStartedEvent()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Confirm();
        appointment.CheckIn();
        appointment.ClearDomainEvents();

        appointment.Start();

        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentStartedEvent);
    }

    [Fact]
    public void Complete_WhenInProgress_ShouldChangeStatusToCompleted()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Confirm();
        appointment.CheckIn();
        appointment.Start();

        appointment.Complete();

        appointment.Status.Should().Be(AppointmentStatus.Completed);
        appointment.ActualEndTime.Should().NotBeNull();
    }

    [Fact]
    public void Complete_ShouldRaiseAppointmentCompletedEvent()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Confirm();
        appointment.CheckIn();
        appointment.Start();
        appointment.ClearDomainEvents();

        appointment.Complete();

        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentCompletedEvent);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(30)]
    public void Reschedule_WithFutureDate_ShouldUpdateScheduledAt(int daysFromNow)
    {
        var appointment = new AppointmentTestBuilder().Build();
        var newDate = DateTime.UtcNow.AddDays(daysFromNow);

        appointment.Reschedule(newDate);

        appointment.ScheduledAt.Should().BeCloseTo(newDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Reschedule_ShouldResetStatusToScheduled()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.Confirm();

        appointment.Reschedule(DateTime.UtcNow.AddDays(3));

        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void Reschedule_ShouldRaiseAppointmentRescheduledEvent()
    {
        var appointment = new AppointmentTestBuilder().Build();
        var oldDate = appointment.ScheduledAt;
        var newDate = DateTime.UtcNow.AddDays(5);
        appointment.ClearDomainEvents();

        appointment.Reschedule(newDate);

        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentRescheduledEvent);
        var evt = appointment.DomainEvents.OfType<AppointmentRescheduledEvent>().First();
        evt.NewDateTime.Should().BeCloseTo(newDate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Reschedule_WithPastDate_ShouldThrowInvalidAppointmentStatusException()
    {
        var appointment = new AppointmentTestBuilder().Build();
        var pastDate = DateTime.UtcNow.AddDays(-1);

        var act = () => appointment.Reschedule(pastDate);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MarkNoShow_WhenScheduled_ShouldChangeStatusToNoShow()
    {
        var appointment = new AppointmentTestBuilder().Build();

        appointment.MarkNoShow();

        appointment.Status.Should().Be(AppointmentStatus.NoShow);
    }

    [Fact]
    public void MarkNoShow_ShouldRaiseAppointmentNoShowEvent()
    {
        var appointment = new AppointmentTestBuilder().Build();
        appointment.ClearDomainEvents();

        appointment.MarkNoShow();

        appointment.DomainEvents.Should().ContainSingle(e => e is AppointmentNoShowEvent);
    }

    [Fact]
    public void FullLifecycle_ShouldTransitionThroughAllStatuses()
    {
        var appointment = new AppointmentTestBuilder().Build();

        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.Confirm();
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.CheckIn();
        appointment.Status.Should().Be(AppointmentStatus.CheckedIn);
        appointment.Start();
        appointment.Status.Should().Be(AppointmentStatus.InProgress);
        appointment.Complete();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Appointment_ShouldHaveRowVersion_ForConcurrencyControl()
    {
        var appointment = new AppointmentTestBuilder().Build();

        appointment.RowVersion.Should().NotBeNull();
    }
}
