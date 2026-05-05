// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Common\AuditableEntity.cs
namespace MediQueue.Domain.Common;

/// <summary>
/// Represents an entity with audit information.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>
    /// Gets the identifier of the user who created the entity.
    /// </summary>
    public string? CreatedBy { get; private set; }

    /// <summary>
    /// Gets the identifier of the user who last updated the entity.
    /// </summary>
    public string? UpdatedBy { get; private set; }

    /// <summary>
    /// Sets the identifier of the user who created the entity.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public void SetCreatedBy(string userId)
    {
        CreatedBy = userId;
    }

    /// <summary>
    /// Sets the identifier of the user who last updated the entity.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    public void SetUpdatedBy(string userId)
    {
        UpdatedBy = userId;
        SetUpdated();
    }
}
