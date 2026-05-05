// Path: MediQueue.API/Controllers/NotificationsController.cs
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Common;
using MediQueue.Application.Notifications.Commands;
using MediQueue.Application.Notifications.DTOs;
using MediQueue.Application.Notifications.Queries;

namespace MediQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender)
    {
        _sender = sender;
    }

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetNotifications([FromQuery] int limit = 50)
    {
        var result = await _sender.Send(new GetNotificationsQuery(UserId, limit));
        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        var result = await _sender.Send(new MarkNotificationAsReadCommand(id));
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
