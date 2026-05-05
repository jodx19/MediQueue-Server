// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Common\BaseEntity.cs
using System;

namespace MediQueue.Domain.Common;

/// <summary>
/// Represents the base class for all domain entities.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Gets the date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the entity is deleted (soft delete).
    /// </summary>
    public bool IsDeleted { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntity"/> class.
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the entity as soft deleted.
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        SetUpdated();
    }

    /// <summary>
    /// Sets the updated date and time to the current UTC time.
    /// </summary>
    public void SetUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
