using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MediQueue.Application.Interfaces;

/// <summary>
/// Checks drug-drug interactions using an AI/clinical API.
/// </summary>
public interface IDrugInteractionService
{
    /// <summary>
    /// Checks whether <paramref name="newDrugName"/> interacts with any of the
    /// currently prescribed drugs in <paramref name="currentDrugNames"/>.
    /// </summary>
    /// <param name="currentDrugNames">Drugs already on the patient's active prescription.</param>
    /// <param name="newDrugName">The new drug being considered.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="DrugInteractionResult"/> describing any found interactions.</returns>
    Task<DrugInteractionResult> CheckAsync(
        IEnumerable<string> currentDrugNames,
        string newDrugName,
        CancellationToken cancellationToken = default);
}

/// <summary>Result returned by <see cref="IDrugInteractionService.CheckAsync"/>.</summary>
public class DrugInteractionResult
{
    public bool HasInteractions { get; init; }
    public List<DrugInteraction> Interactions { get; init; } = [];
    public string Summary { get; init; } = string.Empty;
}

public class DrugInteraction
{
    /// <summary>The existing drug that interacts with the new drug.</summary>
    public string Drug1 { get; init; } = string.Empty;

    /// <summary>The new drug being prescribed.</summary>
    public string Drug2 { get; init; } = string.Empty;

    /// <summary>Severity: Minor | Moderate | Major | Contraindicated</summary>
    public string Severity { get; init; } = string.Empty;

    /// <summary>Clinical description of the interaction.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Recommended clinical action.</summary>
    public string Recommendation { get; init; } = string.Empty;
}
