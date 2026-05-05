// Path: MediQueue.Domain/Entities/Notification.cs
using System;
using MediQueue.Domain.Common;

namespace MediQueue.Domain.Entities;

public enum NotificationType
{
    Information = 1,
    Success = 2,
    Warning = 3,
    Error = 4,
    AppointmentReminder = 5,
    InvoiceOverdue = 6
}

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public NotificationType Type { get; private set; }
    public bool IsRead { get; private set; }
    public new DateTime CreatedAt { get; private set; }

    private Notification() 
    { 
        Title = null!;
        Message = null!;
    }

    private Notification(Guid userId, string title, string message, NotificationType type)
    {
        UserId = userId;
        Title = title;
        Message = message;
        Type = type;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
    }

    public static Notification Create(Guid userId, string title, string message, NotificationType type = NotificationType.Information)
    {
        return new Notification(userId, title, message, type);
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
