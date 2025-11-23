namespace CryptoTracker.Core.Functional;

/// <summary>
/// Unit type representing "no value" in functional programming.
/// Used as a return type for operations that have side effects but no meaningful return value.
/// Equivalent to void, but as a value type that can be used in generic contexts.
/// </summary>
public readonly struct Unit
{
    /// <summary>
    /// Singleton instance of Unit.
    /// </summary>
    public static readonly Unit Value = new();

    /// <summary>
    /// Compares two Unit values (always equal).
    /// </summary>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Returns hash code for Unit (constant).
    /// </summary>
    public override int GetHashCode() => 0;

    /// <summary>
    /// String representation of Unit.
    /// </summary>
    public override string ToString() => "()";

    /// <summary>
    /// Equality operator for Unit values.
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Inequality operator for Unit values.
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;
}
