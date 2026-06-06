using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Tenants.Commands;
using MediQueue.Application.Tenants.Queries;

namespace MediQueue.API.Controllers;

public class TenantsController : BaseApiController
{
    // POST api/tenants/provision
    // Public endpoint — called during clinic signup
    [HttpPost("provision")]
    [AllowAnonymous]
    public async Task<IActionResult> Provision([FromBody] ProvisionTenantCommand command)
    {
        var result = await Sender.Send(command);
        return HandleResult(result);
    }

    // GET api/tenants/{subdomain}/available
    // Check if subdomain is available
    [HttpGet("{subdomain}/available")]
    [AllowAnonymous]
    public async Task<IActionResult> CheckSubdomain(string subdomain)
    {
        var result = await Sender.Send(new CheckSubdomainQuery(subdomain));
        return HandleResult(result);
    }

    // GET api/tenants — SuperAdmin only
    [HttpGet]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> GetAll()
    {
        var result = await Sender.Send(new GetAllTenantsQuery());
        return HandleResult(result);
    }

    // PUT api/tenants/{id}/suspend — SuperAdmin only
    [HttpPut("{id}/suspend")]
    [Authorize(Policy = "SuperAdminOnly")]
    public async Task<IActionResult> Suspend(Guid id)
    {
        var result = await Sender.Send(new SuspendTenantCommand(id));
        return HandleResult(result);
    }
}
