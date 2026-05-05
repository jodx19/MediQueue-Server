// Path: MediQueue.Application/Notifications/Commands/MarkNotificationAsReadCommand.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Notifications.Commands;

public record MarkNotificationAsReadCommand(Guid NotificationId) : IRequest<Result>;

public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _unitOfWork.Notifications.GetByIdAsync(request.NotificationId);
        if (notification == null) return Result.Failure("Notification not found.");

        notification.MarkAsRead();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
