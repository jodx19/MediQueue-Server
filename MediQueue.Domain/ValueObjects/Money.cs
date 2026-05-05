// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\ValueObjects\Money.cs
using System;

namespace MediQueue.Domain.ValueObjects;

/// <summary>
/// Represents a monetary value as an immutable value object.
/// </summary>
public sealed class Money : IEquatable<Money>, IComparable<Money>
{
    /// <summary>
    /// Gets the monetary amount.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the currency code. Default is EGP.
    /// </summary>
    public string Currency { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Money"/> class.
    /// </summary>
    public Money(decimal amount, string currency = "EGP")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency cannot be empty.", nameof(currency));

        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
    }

    private Money() 
    { 
        Currency = "EGP"; // Default
    }

    /// <summary>
    /// Adds another money object to this one.
    /// </summary>
    public Money Add(Money money)
    {
        if (Currency != money.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies.");

        return new Money(Amount + money.Amount, Currency);
    }

    /// <summary>
    /// Subtracts another money object from this one.
    /// </summary>
    public Money Subtract(Money money)
    {
        if (Currency != money.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies.");

        return new Money(Amount - money.Amount, Currency);
    }

    /// <summary>
    /// Multiplies the money amount by a multiplier.
    /// </summary>
    public Money Multiply(decimal multiplier)
    {
        return new Money(Amount * multiplier, Currency);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Money other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Money? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return Amount == other.Amount &&
               Currency == other.Currency;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }

    /// <inheritdoc/>
    public int CompareTo(Money? other)
    {
        if (other is null) return 1;
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies.");

        return Amount.CompareTo(other.Amount);
    }

    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money left, decimal multiplier) => left.Multiply(multiplier);
    public static Money operator *(decimal multiplier, Money right) => right.Multiply(multiplier);

    public static bool operator ==(Money? left, Money? right)
    {
        if (left is null) return right is null;
        return left.Equals(right);
    }

    public static bool operator !=(Money? left, Money? right)
    {
        return !(left == right);
    }

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;
}
