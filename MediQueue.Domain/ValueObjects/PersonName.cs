// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\ValueObjects\PersonName.cs
using System;
using System.Collections.Generic;

namespace MediQueue.Domain.ValueObjects;

/// <summary>
/// Represents a person's name as an immutable value object.
/// </summary>
public sealed class PersonName : IEquatable<PersonName>
{
    /// <summary>
    /// Gets the first name.
    /// </summary>
    public string FirstName { get; }

    /// <summary>
    /// Gets the last name.
    /// </summary>
    public string LastName { get; }

    /// <summary>
    /// Gets the middle name.
    /// </summary>
    public string? MiddleName { get; }

    /// <summary>
    /// Gets the computed full name.
    /// </summary>
    public string FullName => string.IsNullOrWhiteSpace(MiddleName) 
        ? $"{FirstName} {LastName}".Trim()
        : $"{FirstName} {MiddleName} {LastName}".Trim();

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonName"/> class.
    /// </summary>
    /// <param name="firstName">The first name.</param>
    /// <param name="lastName">The last name.</param>
    /// <param name="middleName">The middle name (optional).</param>
    public PersonName(string firstName, string lastName, string? middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
            
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        MiddleName = middleName?.Trim();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PersonName other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(PersonName? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return FirstName == other.FirstName &&
               LastName == other.LastName &&
               MiddleName == other.MiddleName;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(FirstName, LastName, MiddleName);
    }

    /// <summary>
    /// Checks if two PersonName objects are equal.
    /// </summary>
    public static bool operator ==(PersonName? left, PersonName? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Checks if two PersonName objects are not equal.
    /// </summary>
    public static bool operator !=(PersonName? left, PersonName? right)
    {
        return !(left == right);
    }
}
