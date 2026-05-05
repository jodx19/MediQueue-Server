// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\ValueObjects\ContactInfo.cs
using System;

namespace MediQueue.Domain.ValueObjects;

/// <summary>
/// Represents contact information as an immutable value object.
/// </summary>
public sealed class ContactInfo : IEquatable<ContactInfo>
{
    /// <summary>
    /// Gets the primary phone number.
    /// </summary>
    public string Phone { get; }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public string? Email { get; }

    /// <summary>
    /// Gets the alternative phone number.
    /// </summary>
    public string? AlternativePhone { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactInfo"/> class.
    /// </summary>
    /// <param name="phone">The primary phone number.</param>
    /// <param name="email">The email address (optional).</param>
    /// <param name="alternativePhone">The alternative phone number (optional).</param>
    public ContactInfo(string phone, string? email = null, string? alternativePhone = null)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty.", nameof(phone));

        Phone = phone.Trim();
        Email = email?.Trim();
        AlternativePhone = alternativePhone?.Trim();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is ContactInfo other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(ContactInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Phone == other.Phone &&
               Email == other.Email &&
               AlternativePhone == other.AlternativePhone;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Phone, Email, AlternativePhone);
    }

    public static bool operator ==(ContactInfo? left, ContactInfo? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(ContactInfo? left, ContactInfo? right)
    {
        return !(left == right);
    }
}
