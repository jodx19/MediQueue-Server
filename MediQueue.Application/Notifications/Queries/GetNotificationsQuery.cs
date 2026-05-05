// Path: MediQueue.Application/Notifications/Queries/GetNotificationsQuery.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediQueue.Application.Common;
using MediQueue.Application.Notifications.DTOs;
using MediQueue.Domain.Interfaces;

namespace MediQueue.Application.Notifications.Queries;

public record GetNotificationsQuery(Guid UserId, int Limit = 50) : IRequest<Result<List<NotificationDto>>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, Result<List<NotificationDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetNotificationsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var notifications = await _unitOfWork.Notifications.GetUserNotificationsAsync(request.UserId, request.Limit);
        var dtos = notifications.Select(n => new NotificationDto(n.Id, n.Title, n.Message, n.Type, n.IsRead, n.CreatedAt)).ToList();
        return Result<List<NotificationDto>>.Success(dtos);
    }
}
