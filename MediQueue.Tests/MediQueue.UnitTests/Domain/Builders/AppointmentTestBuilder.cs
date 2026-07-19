using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;

namespace MediQueue.UnitTests.Domain.Builders;

public class AppointmentTestBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _patientId = Guid.NewGuid();
    private Guid _doctorId = Guid.NewGuid();
    private Guid _clinicId = Guid.NewGuid();
    private DateTime _scheduledAt = DateTime.UtcNow.AddDays(1);
    private int _durationMinutes = 30;
    private AppointmentPriority _priority = AppointmentPriority.Routine;
    private VisitType _visitType = VisitType.InPerson;
    private string _chiefComplaint = "Routine checkup examination";
    private string? _notes;

    public AppointmentTestBuilder WithId(Guid id) { _id = id; return this; }
    public AppointmentTestBuilder WithPatientId(Guid id) { _patientId = id; return this; }
    public AppointmentTestBuilder WithDoctorId(Guid id) { _doctorId = id; return this; }
    public AppointmentTestBuilder WithClinicId(Guid id) { _clinicId = id; return this; }
    public AppointmentTestBuilder WithScheduledAt(DateTime date) { _scheduledAt = date; return this; }
    public AppointmentTestBuilder WithDurationMinutes(int minutes) { _durationMinutes = minutes; return this; }
    public AppointmentTestBuilder WithPriority(AppointmentPriority priority) { _priority = priority; return this; }
    public AppointmentTestBuilder WithVisitType(VisitType type) { _visitType = type; return this; }
    public AppointmentTestBuilder WithChiefComplaint(string complaint) { _chiefComplaint = complaint; return this; }
    public AppointmentTestBuilder WithNotes(string? notes) { _notes = notes; return this; }

    public Appointment Build()
    {
        var appointment = Appointment.Book(
            _patientId,
            _doctorId,
            _clinicId,
            _scheduledAt,
            _durationMinutes,
            _priority,
            _visitType,
            _chiefComplaint,
            _notes);

        var field = typeof(MediQueue.Domain.Common.BaseEntity)
            .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        field?.SetValue(appointment, _id);

        appointment.ClearDomainEvents();
        return appointment;
    }
}
