// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\ValueObjects\Address.cs
using System;

namespace MediQueue.Domain.ValueObjects;

/// <summary>
/// Represents a physical address as an immutable value object.
/// </summary>
public sealed class Address : IEquatable<Address>
{
    /// <summary>
    /// Gets the street address.
    /// </summary>
    public string Street { get; }

    /// <summary>
    /// Gets the city.
    /// </summary>
    public string City { get; }

    /// <summary>
    /// Gets the governorate or state.
    /// </summary>
    public string Governorate { get; }

    /// <summary>
    /// Gets the country.
    /// </summary>
    public string Country { get; }

    /// <summary>
    /// Gets the postal code.
    /// </summary>
    public string? PostalCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Address"/> class.
    /// </summary>
    public Address(string street, string city, string governorate, string country = "Egypt", string? postalCode = null)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty.", nameof(street));
            
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));
            
        if (string.IsNullOrWhiteSpace(governorate))
            throw new ArgumentException("Governorate cannot be empty.", nameof(governorate));
            
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty.", nameof(country));

        Street = street.Trim();
        City = city.Trim();
        Governorate = governorate.Trim();
        Country = country.Trim();
        PostalCode = postalCode?.Trim();
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Address other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Address? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Street == other.Street &&
               City == other.City &&
               Governorate == other.Governorate &&
               Country == other.Country &&
               PostalCode == other.PostalCode;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Street, City, Governorate, Country, PostalCode);
    }

    public static bool operator ==(Address? left, Address? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Address? left, Address? right)
    {
        return !(left == right);
    }
}
