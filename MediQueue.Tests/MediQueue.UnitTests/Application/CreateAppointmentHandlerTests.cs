using AutoMapper;
using FluentAssertions;
using Moq;
using MediQueue.Application.Appointments.Commands;
using MediQueue.Application.Appointments.DTOs;
using MediQueue.Application.Interfaces;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.Interfaces;
using MediQueue.Domain.ValueObjects;

namespace MediQueue.UnitTests.Application;

public class CreateAppointmentHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly BookAppointmentCommandHandler _handler;
    private readonly BookAppointmentCommand _validCommand;
    private readonly CancellationToken _ct = CancellationToken.None;

    public CreateAppointmentHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _cacheServiceMock = new Mock<ICacheService>();
        _handler = new BookAppointmentCommandHandler(
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _cacheServiceMock.Object);

        _validCommand = new BookAppointmentCommand
        {
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            ClinicId = Guid.NewGuid(),
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            DurationMinutes = 30,
            VisitType = VisitType.InPerson,
            Priority = AppointmentPriority.Routine,
            ChiefComplaint = "Routine dental checkup and cleaning"
        };

        var patient = Patient.Register(
            new PersonName("Test", "User", "Patient"),
            new DateOnly(1990, 1, 1),
            Gender.Male,
            BloodType.OPos,
            "12345678901234",
            new ContactInfo("01012345678"),
            new Domain.ValueObjects.Address("St", "City", "Gov", "Egypt"),
            MaritalStatus.Single);

        var doctor = new Doctor(
            Guid.NewGuid(),
            new PersonName("Doctor", "", "Smith"),
            "General Dentistry",
            MedicalSpecialty.GeneralDentistry,
            "D123",
            new ContactInfo("01112345678"));

        var doctorType = typeof(Doctor);
        var isAvailableProp = doctorType.GetProperty("IsAvailable");
        isAvailableProp?.SetValue(doctor, true);

        var appointment = Appointment.Book(
            _validCommand.PatientId,
            _validCommand.DoctorId,
            _validCommand.ClinicId,
            _validCommand.ScheduledAt,
            _validCommand.DurationMinutes,
            _validCommand.Priority,
            _validCommand.VisitType,
            _validCommand.ChiefComplaint);

        _unitOfWorkMock.Setup(u => u.Patients.GetByIdAsync(_validCommand.PatientId))
            .ReturnsAsync(patient);

        _unitOfWorkMock.Setup(u => u.Doctors.GetByIdAsync(_validCommand.DoctorId))
            .ReturnsAsync(doctor);

        _unitOfWorkMock.Setup(u => u.Appointments.HasConflictAsync(
                _validCommand.DoctorId, _validCommand.ScheduledAt, _validCommand.DurationMinutes))
            .ReturnsAsync(false);

        _mapperMock.Setup(m => m.Map<AppointmentDto>(It.IsAny<Appointment>()))
            .Returns(new AppointmentDto
            {
                Id = appointment.Id,
                PatientId = _validCommand.PatientId,
                DoctorId = _validCommand.DoctorId,
                Status = AppointmentStatus.Scheduled
            });
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateAppointment_AndReturnSuccess()
    {
        var result = await _handler.Handle(_validCommand, _ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public async Task Handle_WhenPatientNotFound_ShouldReturnFailure()
    {
        _unitOfWorkMock.Setup(u => u.Patients.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Patient?)null);

        var result = await _handler.Handle(_validCommand, _ct);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Patient not found");
    }

    [Fact]
    public async Task Handle_WhenPatientIsInactive_ShouldReturnFailure()
    {
        var inactivePatient = Patient.Register(
            new PersonName("Inactive", "", "Patient"),
            new DateOnly(1985, 5, 5),
            Gender.Female,
            BloodType.ANeg,
            "98765432109876",
            new ContactInfo("01212345678"),
            new Domain.ValueObjects.Address("St", "City", "Gov", "Egypt"),
            MaritalStatus.Married);
        inactivePatient.Deactivate();

        _unitOfWorkMock.Setup(u => u.Patients.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(inactivePatient);

        var result = await _handler.Handle(_validCommand, _ct);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("inactive");
    }

    [Fact]
    public async Task Handle_WhenDoctorNotFound_ShouldReturnFailure()
    {
        _unitOfWorkMock.Setup(u => u.Doctors.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Doctor?)null);

        var result = await _handler.Handle(_validCommand, _ct);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Doctor not found");
    }

    [Fact]
    public async Task Handle_WhenDoctorUnavailable_ShouldReturnFailure()
    {
        var doctor = new Doctor(
            Guid.NewGuid(),
            new PersonName("Unavailable", "", "Doc"),
            "Dentistry",
            MedicalSpecialty.GeneralDentistry,
            "D456",
            new ContactInfo("01198765432"));
        var isAvailableProp = typeof(Doctor).GetProperty("IsAvailable");
        isAvailableProp?.SetValue(doctor, false);

        _unitOfWorkMock.Setup(u => u.Doctors.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(doctor);

        var result = await _handler.Handle(_validCommand, _ct);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenDoctorUnavailable_ShouldConflict_ShouldReturnFailure()
    {
        _unitOfWorkMock.Setup(u => u.Appointments.HasConflictAsync(
                _validCommand.DoctorId, _validCommand.ScheduledAt, _validCommand.DurationMinutes))
            .ReturnsAsync(true);

        var result = await _handler.Handle(_validCommand, _ct);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_OnSuccess_ShouldCallSaveChangesOnce()
    {
        await _handler.Handle(_validCommand, _ct);

        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(_ct), Times.Once);
    }

    [Fact]
    public async Task Handle_OnSuccess_ShouldAddAppointment()
    {
        await _handler.Handle(_validCommand, _ct);

        _unitOfWorkMock.Verify(u => u.Appointments.AddAsync(It.IsAny<Appointment>()), Times.Once);
    }

    [Fact]
    public async Task Handle_OnSuccess_ShouldEvictAvailabilityCache()
    {
        await _handler.Handle(_validCommand, _ct);

        _cacheServiceMock.Verify(c => c.RemoveAsync(
            It.Is<string>(s => s.Contains(_validCommand.DoctorId.ToString()))), Times.Once);
    }
}
