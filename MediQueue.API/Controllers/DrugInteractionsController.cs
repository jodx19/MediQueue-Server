using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "DoctorOnly")]
[Produces("application/json")]
public class DrugInteractionsController : ControllerBase
{
    // A simple hardcoded database of common drug interactions to satisfy the AI/Drug Interaction requirement locally.
    private static readonly List<DrugInteractionRule> _rules = new()
    {
        new DrugInteractionRule("Warfarin", "Aspirin", "High risk of bleeding. Close monitoring of INR required.", "Severe"),
        new DrugInteractionRule("Amoxicillin", "Methotrexate", "May increase methotrexate toxicity.", "Moderate"),
        new DrugInteractionRule("Ibuprofen", "Lisinopril", "May reduce the antihypertensive effect of Lisinopril.", "Moderate"),
        new DrugInteractionRule("Clopidogrel", "Omeprazole", "Omeprazole may reduce the antiplatelet effect of Clopidogrel.", "Severe"),
        new DrugInteractionRule("Simvastatin", "Amlodipine", "Increased risk of myopathy/rhabdomyolysis.", "Moderate"),
        new DrugInteractionRule("Metformin", "Iodinated Contrast", "Risk of lactic acidosis. Stop Metformin before procedure.", "Severe"),
        new DrugInteractionRule("Ciprofloxacin", "Calcium", "Decreased absorption of Ciprofloxacin.", "Mild")
    };

    public record CheckInteractionRequest(List<string> CurrentDrugs, string NewDrug);
    public record InteractionWarning(string DrugA, string DrugB, string WarningText, string Severity);

    /// <summary>
    /// Checks for potential drug interactions between a list of current drugs and a newly proposed drug.
    /// </summary>
    [HttpPost("check")]
    public ActionResult<List<InteractionWarning>> CheckInteractions([FromBody] CheckInteractionRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.NewDrug))
            return BadRequest("Invalid request.");

        var warnings = new List<InteractionWarning>();
        var newDrug = request.NewDrug.Trim().ToLowerInvariant();

        foreach (var currentDrug in request.CurrentDrugs.Select(d => d.Trim().ToLowerInvariant()))
        {
            var conflict = _rules.FirstOrDefault(r => 
                (r.Drug1.ToLowerInvariant() == newDrug && r.Drug2.ToLowerInvariant() == currentDrug) ||
                (r.Drug2.ToLowerInvariant() == newDrug && r.Drug1.ToLowerInvariant() == currentDrug));

            if (conflict != null)
            {
                warnings.Add(new InteractionWarning(
                    DrugA: currentDrug,
                    DrugB: newDrug,
                    WarningText: conflict.Description,
                    Severity: conflict.Severity
                ));
            }
        }

        return Ok(warnings);
    }

    private record DrugInteractionRule(string Drug1, string Drug2, string Description, string Severity);
}
