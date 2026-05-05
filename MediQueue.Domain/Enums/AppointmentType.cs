namespace MediQueue.Domain.Enums;

/// <summary>
/// Represents the type of a dental appointment.
/// </summary>
public enum AppointmentType
{
    /// <summary>General checkup</summary>
    Checkup = 1,
    /// <summary>Teeth cleaning</summary>
    Cleaning = 2,
    /// <summary>Cavity filling</summary>
    Filling = 3,
    /// <summary>Root canal treatment</summary>
    RootCanal = 4,
    /// <summary>Tooth extraction</summary>
    Extraction = 5,
    /// <summary>Orthodontic procedure or adjustment</summary>
    Orthodontic = 6,
    /// <summary>Dental implant procedure</summary>
    Implant = 7,
    /// <summary>Emergency dental care</summary>
    Emergency = 8
}
