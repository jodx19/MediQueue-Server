using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediQueue.Application.Tenants.Commands;
using MediQueue.Application.Tenants.Queries;

namespace MediQueue.API.Controllers;

[Route("api/tenants")]
[ApiController]
public class TenantsController : BaseApiController
{
    /// <summary>
    /// Provisions a new tenant (clinic) — public endpoint
    /// </summary>
    [HttpPost("provision")]
    [AllowAnonymous]
    [EnableRateLimiting("TenantProvisionPolicy")]
    [ProducesResponseType(typeof(ProvisionTenantResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> Provision([FromBody] ProvisionTenantCommand command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Checks if a subdomain is available — public endpoint
    /// </summary>
    [HttpGet("{subdomain}/available")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CheckSubdomainResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckSubdomain(string subdomain)
    {
        var result = await Sender.Send(new CheckSubdomainQuery(subdomain));
        return HandleResult(result);
    }

    /// <summary>
    /// Gets all tenants — SuperAdmin only
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "SuperAdminOnly")]
    [ProducesResponseType(typeof(TenantDto[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll()
    {
        var result = await Sender.Send(new GetAllTenantsQuery());
        return HandleResult(result);
    }

    /// <summary>
    /// Suspends a tenant — SuperAdmin only
    /// </summary>
    [HttpPut("{id}/suspend")]
    [Authorize(Policy = "SuperAdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid id)
    {
        var result = await Sender.Send(new SuspendTenantCommand(id));
        return HandleResult(result);
    }

    /// <summary>
    /// Activates a suspended tenant — SuperAdmin only
    /// </summary>
    [HttpPut("{id}/activate")]
    [Authorize(Policy = "SuperAdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id)
    {
        var result = await Sender.Send(new ActivateTenantCommand(id));
        return HandleResult(result);
    }
}
