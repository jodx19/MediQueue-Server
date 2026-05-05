// Path: MediQueue.Infrastructure/Persistence/Repositories/NotificationRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediQueue.Domain.Entities;
using MediQueue.Domain.Interfaces;
using MediQueue.Infrastructure.Persistence.Context;

namespace MediQueue.Infrastructure.Persistence.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly ClinicDbContext _context;

    public NotificationRepository(ClinicDbContext context)
    {
        _context = context;
    }

    public async Task<Notification?> GetByIdAsync(Guid id)
    {
        return await _context.Set<Notification>().FindAsync(id);
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId, int limit = 50)
    {
        return await _context.Set<Notification>()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _context.Set<Notification>()
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task AddAsync(Notification notification)
    {
        await _context.Set<Notification>().AddAsync(notification);
    }

    public async Task UpdateAsync(Notification notification)
    {
        _context.Set<Notification>().Update(notification);
        await Task.CompletedTask;
    }

    public async Task MarkAllAsReadAsync(Guid userId)
    {
        var unread = await _context.Set<Notification>()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
        }
    }
}
