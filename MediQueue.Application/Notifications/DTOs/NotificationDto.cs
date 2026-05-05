// Path: MediQueue.Application/Notifications/DTOs/NotificationDto.cs
using System;
using MediQueue.Domain.Entities;

namespace MediQueue.Application.Notifications.DTOs;

public record NotificationDto(
    Guid Id,
    string Title,
    string Message,
    NotificationType Type,
    bool IsRead,
    DateTime CreatedAt);
