namespace CryptoTracker.Core.Functional;

/// <summary>
/// Represents an optional value that may or may not be present.
/// This is an immutable type for handling null-safety functionally.
/// </summary>
/// <typeparam name="T">The type of the optional value</typeparam>
public readonly struct Option<T>
{
    private readonly T? _value;
    private readonly bool _hasValue;

    private Option(T? value, bool hasValue)
    {
        _value = value;
        _hasValue = hasValue;
    }

    /// <summary>
    /// Creates an Option with a value
    /// </summary>
    public static Option<T> Some(T value) => new(value, true);

    /// <summary>
    /// Creates an empty Option (None)
    /// </summary>
    public static Option<T> None() => new(default, false);

    /// <summary>
    /// Creates an Option from a nullable value
    /// </summary>
    public static Option<T> FromNullable(T? value) =>
        value != null ? Some(value) : None();

    /// <summary>
    /// Indicates whether the option has a value
    /// </summary>
    public bool IsSome => _hasValue;

    /// <summary>
    /// Indicates whether the option is empty
    /// </summary>
    public bool IsNone => !_hasValue;

    /// <summary>
    /// Gets the value if present, otherwise returns default
    /// </summary>
    public T? Value => _value;

    /// <summary>
    /// Maps the value to a new type using a pure function
    /// </summary>
    public Option<TNew> Map<TNew>(Func<T, TNew> mapper) =>
        _hasValue && _value != null
            ? Option<TNew>.Some(mapper(_value))
            : Option<TNew>.None();

    /// <summary>
    /// Binds (flatMaps) the option to another operation that returns an Option
    /// </summary>
    public Option<TNew> Bind<TNew>(Func<T, Option<TNew>> binder) =>
        _hasValue && _value != null
            ? binder(_value)
            : Option<TNew>.None();

    /// <summary>
    /// Returns the value if present, otherwise returns the provided default value
    /// </summary>
    public T GetOrDefault(T defaultValue) =>
        _hasValue && _value != null ? _value : defaultValue;

    /// <summary>
    /// Returns the value if present, otherwise computes and returns a default value
    /// </summary>
    public T GetOrElse(Func<T> defaultProvider) =>
        _hasValue && _value != null ? _value : defaultProvider();

    /// <summary>
    /// Returns the value if present, otherwise throws an exception
    /// </summary>
    public T GetOrThrow() =>
        _hasValue && _value != null
            ? _value
            : throw new InvalidOperationException("Option has no value");

    /// <summary>
    /// Pattern matching for functional composition
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSome, Func<TResult> onNone) =>
        _hasValue && _value != null
            ? onSome(_value)
            : onNone();

    /// <summary>
    /// Filters the option based on a predicate
    /// </summary>
    public Option<T> Filter(Func<T, bool> predicate) =>
        _hasValue && _value != null && predicate(_value)
            ? this
            : None();

    /// <summary>
    /// Executes an action if the option has a value
    /// </summary>
    public Option<T> OnSome(Action<T> action)
    {
        if (_hasValue && _value != null)
            action(_value);
        return this;
    }

    /// <summary>
    /// Executes an action if the option is empty
    /// </summary>
    public Option<T> OnNone(Action action)
    {
        if (!_hasValue)
            action();
        return this;
    }

    /// <summary>
    /// Converts Option to Result with a provided error message for None case
    /// </summary>
    public Result<T> ToResult(string errorIfNone) =>
        _hasValue && _value != null
            ? Result<T>.Success(_value)
            : Result<T>.Failure(errorIfNone);
}

/// <summary>
/// Extension methods for working with Option types
/// </summary>
public static class OptionExtensions
{
    /// <summary>
    /// Flattens an IEnumerable of Options, keeping only the Some values
    /// </summary>
    public static IEnumerable<T> Choose<T>(this IEnumerable<Option<T>> options) =>
        options
            .Where(opt => opt.IsSome)
            .Select(opt => opt.Value!);

    /// <summary>
    /// Converts an IEnumerable to Option, returning Some if any elements exist, None otherwise
    /// </summary>
    public static Option<IEnumerable<T>> ToOption<T>(this IEnumerable<T> items)
    {
        var list = items.ToList();
        return list.Any() ? Option<IEnumerable<T>>.Some(list) : Option<IEnumerable<T>>.None();
    }

    /// <summary>
    /// Returns the first element as an Option
    /// </summary>
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> items) =>
        items.Any() ? Option<T>.Some(items.First()) : Option<T>.None();

    /// <summary>
    /// Returns the first element matching a predicate as an Option
    /// </summary>
    public static Option<T> FirstOrNone<T>(this IEnumerable<T> items, Func<T, bool> predicate)
    {
        foreach (var item in items)
        {
            if (predicate(item))
                return Option<T>.Some(item);
        }
        return Option<T>.None();
    }
}
