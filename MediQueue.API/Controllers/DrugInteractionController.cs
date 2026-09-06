using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediQueue.Application.Interfaces;

namespace MediQueue.API.Controllers;

/// <summary>AI-powered drug interaction checking endpoint.</summary>
[ApiController]
[Route("api/drug-interactions")]
[Authorize]
[Produces("application/json")]
public class DrugInteractionController : ControllerBase
{
    private readonly IDrugInteractionService _drugInteractionService;

    public DrugInteractionController(IDrugInteractionService drugInteractionService)
    {
        _drugInteractionService = drugInteractionService;
    }

    /// <summary>
    /// Check whether a new drug interacts with the patient's current medications.
    /// </summary>
    /// <remarks>
    /// Used in the prescriptions tab before adding a new drug to the patient's prescription.
    /// The frontend calls this on "Add Drug" and displays a warning modal if interactions are found.
    /// </remarks>
    [HttpPost("check")]
    [Authorize(Policy = "DoctorOnly")]
    [ProducesResponseType(typeof(DrugInteractionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DrugInteractionResult>> Check(
        [FromBody] DrugInteractionCheckRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.NewDrugName))
            return BadRequest("NewDrugName is required.");

        var result = await _drugInteractionService.CheckAsync(
            request.CurrentDrugNames,
            request.NewDrugName,
            ct);

        return Ok(result);
    }
}

/// <summary>Request body for the drug interaction check endpoint.</summary>
public class DrugInteractionCheckRequest
{
    /// <summary>Drug names currently on the patient's active prescription.</summary>
    public List<string> CurrentDrugNames { get; set; } = [];

    /// <summary>The new drug the doctor wants to add.</summary>
    public string NewDrugName { get; set; } = string.Empty;
}
