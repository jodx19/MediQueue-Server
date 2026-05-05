// Path: MediQueue.Infrastructure/Persistence/Settings/SeedingSettings.cs
namespace MediQueue.Infrastructure.Persistence.Settings;

public class SeedingSettings
{
    public AdminSettings Admin { get; set; } = new();
    public DoctorSettings Doctor { get; set; } = new();
    public ReceptionistSettings Receptionist { get; set; } = new();

    public class AdminSettings
    {
        public string Username { get; set; } = "admin";
        public string Email { get; set; } = "admin@mediqueue.com";
        public string Password { get; set; } = "Admin@123";
    }

    public class DoctorSettings
    {
        public string Username { get; set; } = "dr_ahmed";
        public string Email { get; set; } = "ahmed.k@mediqueue.com";
        public string Password { get; set; } = "Doctor@123";
    }

    public class ReceptionistSettings
    {
        public string Username { get; set; } = "reception";
        public string Email { get; set; } = "reception@mediqueue.com";
        public string Password { get; set; } = "Staff@123";
    }
}
