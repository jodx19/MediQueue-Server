using System;

namespace MediQueue.Infrastructure.Persistence.Entities;

public class ClinicSettings
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ClinicName { get; set; } = string.Empty;
    public string ClinicPhone { get; set; } = string.Empty;
    public string ClinicEmail { get; set; } = string.Empty;
    public string ClinicAddress { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public TimeOnly WorkStartTime { get; set; } = new(8, 0);
    public TimeOnly WorkEndTime { get; set; } = new(17, 0);
    public int AppointmentDurationMinutes { get; set; } = 30;
    public string Currency { get; set; } = "EGP";
    public string TimeZone { get; set; } = "Egypt Standard Time";
    public bool AllowOnlineBooking { get; set; } = true;
    public bool RequireDepositForBooking { get; set; } = false;
    public decimal DepositAmount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Step 10 ready: TenantId will be added here
    public Guid TenantId { get; set; }
}
