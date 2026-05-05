// Path: MediQueue.Domain/Interfaces/INotificationRepository.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediQueue.Domain.Entities;

namespace MediQueue.Domain.Interfaces;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int limit = 50);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
    Task MarkAllAsReadAsync(Guid userId);
}
