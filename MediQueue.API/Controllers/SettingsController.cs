using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Settings.Commands;
using MediQueue.Application.Settings.Dtos;
using MediQueue.Application.Settings.Queries;

namespace MediQueue.API.Controllers;

[Route("api/settings")]
[ApiController]
public class SettingsController : BaseApiController
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ClinicSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSettings()
    {
        var result = await Sender.Send(new GetSettingsQuery());
        return HandleResult(result);
    }

    [HttpPut]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ClinicSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateSettingsCommand command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }
}
