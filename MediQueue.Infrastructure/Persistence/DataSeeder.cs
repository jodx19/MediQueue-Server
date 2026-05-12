using MediQueue.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediQueue.Domain.Common;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.ValueObjects;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Persistence;

public class DataSeeder : IDataSeeder
{
    private static readonly Guid AdminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid DoctorUserId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid ReceptionUserId = Guid.Parse("00000000-0000-0000-0000-000000000003");

    private static readonly Guid DoctorAhmedId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    private static readonly Guid DoctorSaraId = Guid.Parse("00000000-0000-0000-0000-000000000011");
    private static readonly Guid DoctorKhaledId = Guid.Parse("00000000-0000-0000-0000-000000000012");

    private static readonly Guid PatientMohamedId = Guid.Parse("00000000-0000-0000-0000-000000000020");
    private static readonly Guid PatientFatmaId = Guid.Parse("00000000-0000-0000-0000-000000000021");
    private static readonly Guid PatientOmarId = Guid.Parse("00000000-0000-0000-0000-000000000022");
    private static readonly Guid PatientNourId = Guid.Parse("00000000-0000-0000-0000-000000000023");
    private static readonly Guid PatientYoussefId = Guid.Parse("00000000-0000-0000-0000-000000000024");

    private static readonly Guid ClinicId = Guid.Parse("00000000-0000-0000-0000-000000000100");

    private readonly ClinicDbContext _context;
    private readonly IPasswordHasher<AppUser> _hasher;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(
        ClinicDbContext context,
        IPasswordHasher<AppUser> hasher,
        ILogger<DataSeeder> logger)
    {
        _context = context;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Seed skipped because users already exist.");
            return;
        }

        await SeedUsersAsync();
        await SeedDoctorsAsync();
        await SeedPatientsAsync();
        await SeedAppointmentsAsync();
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seed data applied successfully.");
    }

    private async Task SeedUsersAsync()
    {
        var users = new[]
        {
            CreateUser(
                AdminUserId,
                "admin",
                "admin@mediqueue.com",
                "Admin",
                "System",
                "01000000000",
                "Admin@123456",
                UserRole.Admin),
            CreateUser(
                Guid.NewGuid(),
                "ma7moud",
                "ma7moudmostafa19@gmail.com",
                "Mahmoud",
                "Mostafa",
                "01099999999",
                "Admin@123456",
                UserRole.Admin),
            CreateUser(
                DoctorUserId,
                "dr.ahmed",
                "dr.ahmed@mediqueue.com",
                "Ahmed",
                "Hassan",
                "01010000000",
                "Doctor@123456",
                UserRole.Doctor,
                doctorId: DoctorAhmedId),
            CreateUser(
                ReceptionUserId,
                "reception",
                "reception@mediqueue.com",
                "Reception",
                "Desk",
                "01020000000",
                "Recept@123456",
                UserRole.Receptionist)
        };

        await _context.Users.AddRangeAsync(users);
    }

    private AppUser CreateUser(
        Guid id,
        string username,
        string email,
        string firstName,
        string lastName,
        string phone,
        string password,
        UserRole role,
        Guid? doctorId = null,
        Guid? patientId = null)
    {
        var user = AppUser.Create(
            username,
            email,
            firstName,
            lastName,
            phone,
            string.Empty,
            role,
            doctorId,
            patientId);

        user.SetPasswordHash(_hasher.HashPassword(user, password));
        SetEntityId(user, id);
        return user;
    }

    private async Task SeedDoctorsAsync()
    {
        var drAhmed = Doctor.Create(
            new PersonName("Ahmed", "Hassan"),
            MedicalSpecialty.Cardiology,
            "LIC-CARD-001",
            new ContactInfo("01010000000", "dr.ahmed@mediqueue.com"),
            new Money(500),
            new Money(250),
            yearsOfExperience: 12);
        SetEntityId(drAhmed, DoctorAhmedId);

        foreach (var day in new[]
                 {
                     DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday,
                     DayOfWeek.Tuesday, DayOfWeek.Wednesday
                 })
        {
            drAhmed.AddWorkingShift(new WorkingShift(day, new TimeOnly(9, 0), new TimeOnly(17, 0)));
        }

        var drSara = Doctor.Create(
            new PersonName("Sara", "Mohamed"),
            MedicalSpecialty.Pediatrics,
            "LIC-PED-002",
            new ContactInfo("01011000000", "dr.sara@mediqueue.com"),
            new Money(450),
            new Money(225),
            yearsOfExperience: 9);
        SetEntityId(drSara, DoctorSaraId);

        foreach (var day in new[]
                 {
                     DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday,
                     DayOfWeek.Wednesday, DayOfWeek.Thursday
                 })
        {
            drSara.AddWorkingShift(new WorkingShift(day, new TimeOnly(10, 0), new TimeOnly(18, 0)));
        }

        var drKhaled = Doctor.Create(
            new PersonName("Khaled", "Ibrahim"),
            MedicalSpecialty.Orthopedics,
            "LIC-ORTH-003",
            new ContactInfo("01012000000", "dr.khaled@mediqueue.com"),
            new Money(475),
            new Money(230),
            yearsOfExperience: 10);
        SetEntityId(drKhaled, DoctorKhaledId);

        foreach (var day in new[] { DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday })
        {
            drKhaled.AddWorkingShift(new WorkingShift(day, new TimeOnly(8, 0), new TimeOnly(14, 0)));
        }

        await _context.Doctors.AddRangeAsync(drAhmed, drSara, drKhaled);
    }

    private async Task SeedPatientsAsync()
    {
        var mohamed = Patient.Register(
            new PersonName("Mohamed", "Ali"),
            new DateOnly(1985, 3, 15),
            Gender.Male,
            BloodType.APos,
            "28503151234567",
            new ContactInfo("01130000001", "mohamed.ali@demo.local"),
            new Address("12 Tahrir St", "Cairo", "Cairo"),
            MaritalStatus.Married);
        SetEntityId(mohamed, PatientMohamedId);
        mohamed.AddAllergy("Penicillin", MediQueue.Domain.Entities.AllergySeverity.Severe, "Severe allergic reaction");
        mohamed.AddChronicCondition("Hypertension", new DateOnly(2020, 1, 1), "I10");

        var fatma = Patient.Register(
            new PersonName("Fatma", "Hassan"),
            new DateOnly(1990, 7, 22),
            Gender.Female,
            BloodType.OPos,
            "29007221234567",
            new ContactInfo("01130000002", "fatma.hassan@demo.local"),
            new Address("4 Nile Corniche", "Cairo", "Cairo"),
            MaritalStatus.Married);
        SetEntityId(fatma, PatientFatmaId);
        fatma.AddChronicCondition("Type 2 Diabetes", new DateOnly(2019, 6, 1), "E11");

        var omar = Patient.Register(
            new PersonName("Omar", "Samir"),
            new DateOnly(1978, 11, 8),
            Gender.Male,
            BloodType.BNeg,
            "27811081234567",
            new ContactInfo("01130000003", "omar.samir@demo.local"),
            new Address("50 Zamalek Rd", "Cairo", "Cairo"),
            MaritalStatus.Married);
        SetEntityId(omar, PatientOmarId);

        var nour = Patient.Register(
            new PersonName("Nour", "Ahmed"),
            new DateOnly(1995, 4, 30),
            Gender.Female,
            BloodType.ABPos,
            "29504301234567",
            new ContactInfo("01130000004", "nour.ahmed@demo.local"),
            new Address("17 Gamal Abd El Nasser", "Alexandria", "Alexandria"),
            MaritalStatus.Single);
        SetEntityId(nour, PatientNourId);
        nour.AddAllergy("Aspirin", MediQueue.Domain.Entities.AllergySeverity.Moderate, "Moderate skin rash");

        var youssef = Patient.Register(
            new PersonName("Youssef", "Kamal"),
            new DateOnly(2010, 1, 20),
            Gender.Male,
            BloodType.OPos,
            "31001201234567",
            new ContactInfo("01130000005", "youssef.kamal@demo.local"),
            new Address("9 El-Horreya", "Giza", "Giza"),
            MaritalStatus.Single);
        SetEntityId(youssef, PatientYoussefId);

        await _context.Patients.AddRangeAsync(mohamed, fatma, omar, nour, youssef);
    }

    private async Task SeedAppointmentsAsync()
    {
        var today = DateTime.UtcNow.Date;

        var appointment1 = Appointment.Book(
            PatientMohamedId,
            DoctorAhmedId,
            ClinicId,
            today.AddHours(10),
            30,
            AppointmentPriority.Routine,
            VisitType.Consultation,
            "Cardiology follow-up");
        appointment1.Confirm();
        appointment1.ClearDomainEvents();

        var appointment2 = Appointment.Book(
            PatientFatmaId,
            DoctorSaraId,
            ClinicId,
            today.AddHours(11),
            30,
            AppointmentPriority.Routine,
            VisitType.Consultation,
            "Pediatric consultation");
        appointment2.ClearDomainEvents();

        var appointment3 = Appointment.Book(
            PatientOmarId,
            DoctorKhaledId,
            ClinicId,
            today.AddDays(1).AddHours(9),
            30,
            AppointmentPriority.Routine,
            VisitType.FollowUp,
            "Orthopedic follow-up");
        appointment3.ClearDomainEvents();

        await _context.Appointments.AddRangeAsync(appointment1, appointment2, appointment3);
    }

    private static void SetEntityId(BaseEntity entity, Guid id)
    {
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))?.SetValue(entity, id);
    }
}
