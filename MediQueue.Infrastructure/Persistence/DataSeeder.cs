// Path: MediQueue.Infrastructure/Persistence/DataSeeder.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MediQueue.Domain.Common;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Enums;
using MediQueue.Domain.ValueObjects;
using MediQueue.Infrastructure.Persistence.Context;
using MediQueue.Infrastructure.Persistence.Settings;

namespace MediQueue.Infrastructure.Persistence;

public class DataSeeder : IDataSeeder
{
    private readonly ClinicDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly SeedingSettings _settings;

    // Fixed Guids for relationships
    private readonly Guid AdminId = Guid.Parse("A1111111-1111-1111-1111-111111111111");
    private readonly Guid DoctorAhmedId = Guid.Parse("D1111111-1111-1111-1111-111111111111");
    private readonly Guid DoctorAhmedUserId = Guid.Parse("01111111-1111-1111-1111-111111111111");
    private readonly Guid Patient1Id = Guid.Parse("02222222-2222-2222-2222-222222222222");
    private readonly Guid ClinicId = Guid.Parse("03333333-3333-3333-3333-333333333333");

    public DataSeeder(
        ClinicDbContext context, 
        IPasswordHasher<AppUser> passwordHasher,
        IOptions<SeedingSettings> settings)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _settings = settings.Value;
    }

    public async Task SeedAsync()
    {
        Console.WriteLine("--- Starting Database Seeding ---");
        await SeedUsersAsync();
        await SeedDoctorsAsync();
        await SeedPatientsAsync();
        await SeedAppointmentsAsync();

        await _context.SaveChangesAsync();
        Console.WriteLine("--- Database Seeding Completed Successfully ---");
    }

    private async Task SeedUsersAsync()
    {
        if (_context.Users.Any()) return;

        var admin = AppUser.Create(_settings.Admin.Username, _settings.Admin.Email, "", UserRole.Admin);
        admin.SetPasswordHash(_passwordHasher.HashPassword(admin, _settings.Admin.Password));
        typeof(BaseEntity).GetProperty("Id")?.SetValue(admin, AdminId);

        var doctorUser = AppUser.Create(_settings.Doctor.Username, _settings.Doctor.Email, "", UserRole.Doctor, doctorId: DoctorAhmedId);
        doctorUser.SetPasswordHash(_passwordHasher.HashPassword(doctorUser, _settings.Doctor.Password));
        typeof(BaseEntity).GetProperty("Id")?.SetValue(doctorUser, DoctorAhmedUserId);

        var staff = AppUser.Create(_settings.Receptionist.Username, _settings.Receptionist.Email, "", UserRole.Receptionist);
        staff.SetPasswordHash(_passwordHasher.HashPassword(staff, _settings.Receptionist.Password));

        await _context.Users.AddRangeAsync(admin, doctorUser, staff);
    }

    private async Task SeedDoctorsAsync()
    {
        if (_context.Doctors.Any()) return;

        var name = new PersonName("Ahmed", "Kamal", "M.");
        var contact = new ContactInfo("01012345678", "ahmed.k@mediqueue.com");
        var fee = new Money(500, "EGP");
        var followUp = new Money(200, "EGP");

        var drAhmed = Doctor.Create(name, MedicalSpecialty.Cardiology, "LIC-12345", contact, fee, followUp, subSpecialty: "Interventional Cardiology", yearsOfExperience: 15);
        typeof(BaseEntity).GetProperty("Id")?.SetValue(drAhmed, DoctorAhmedId);
        
        drAhmed.AddWorkingShift(new WorkingShift(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 0)));
        drAhmed.AddWorkingShift(new WorkingShift(DayOfWeek.Wednesday, new TimeOnly(9, 0), new TimeOnly(17, 0)));

        var drSara = Doctor.Create(new PersonName("Sara", "Ali"), MedicalSpecialty.Pediatrics, "LIC-67890", new ContactInfo("01122334455"), new Money(400), new Money(150), yearsOfExperience: 8);
        drSara.AddWorkingShift(new WorkingShift(DayOfWeek.Monday, new TimeOnly(10, 0), new TimeOnly(14, 0))); // Overlaps with Ahmed on Monday

        await _context.Doctors.AddRangeAsync(drAhmed, drSara);
    }

    private async Task SeedPatientsAsync()
    {
        if (_context.Patients.Any()) return;

        var patient1 = Patient.Register(
            new PersonName("John", "Doe"),
            new DateOnly(1985, 5, 20),
            Gender.Male,
            BloodType.OPos,
            "28505201234567",
            new ContactInfo("01234567890", "john.doe@email.com"),
            new Address("123 Nile St", "Cairo", "Cairo"),
            MaritalStatus.Married
        );
        typeof(BaseEntity).GetProperty("Id")?.SetValue(patient1, Patient1Id);
        patient1.AddAllergy("Penicillin", MediQueue.Domain.Entities.AllergySeverity.Severe, "Anaphylaxis");
        patient1.AddChronicCondition("Hypertension", diagnosedAt: new DateOnly(2020, 1, 1));

        var patient2 = Patient.Register(
            new PersonName("Mariam", "Zaki"),
            new DateOnly(1992, 10, 12),
            Gender.Female,
            BloodType.ANeg,
            "29210121234568",
            new ContactInfo("01556677889"),
            new Address("45 Giza St", "Giza", "Giza"),
            MaritalStatus.Single
        );

        var patient3 = Patient.Register(
            new PersonName("Youssef", "Mansour"),
            new DateOnly(1978, 3, 15),
            Gender.Male,
            BloodType.BPos,
            "27803151234569",
            new ContactInfo("01288990011"),
            new Address("10 Maadi St", "Cairo", "Cairo"),
            MaritalStatus.Divorced
        );

        await _context.Patients.AddRangeAsync(patient1, patient2, patient3);
    }

    private async Task SeedAppointmentsAsync()
    {
        if (_context.Appointments.Any()) return;

        var today = DateTime.UtcNow.Date;
        
        var appt1 = Appointment.Book(
            Patient1Id,
            DoctorAhmedId,
            ClinicId,
            today.AddHours(10),
            30,
            AppointmentPriority.Routine,
            VisitType.Consultation,
            "Regular heart checkup"
        );

        var appt2 = Appointment.Book(
            Patient1Id,
            DoctorAhmedId,
            ClinicId,
            DateTime.UtcNow.AddDays(1).Date.AddHours(14),
            20,
            AppointmentPriority.Urgent,
            VisitType.FollowUp,
            "Results review"
        );

        await _context.Appointments.AddRangeAsync(appt1, appt2);
    }
}
