namespace CryptoTracker.Core.Functional;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// This is an immutable discriminated union type for railway-oriented programming.
/// </summary>
/// <typeparam name="T">The type of the success value</typeparam>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly string? _error;
    private readonly bool _isSuccess;

    private Result(T value, string? error, bool isSuccess)
    {
        _value = value;
        _error = error;
        _isSuccess = isSuccess;
    }

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    public static Result<T> Success(T value) => new(value, null, true);

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    public static Result<T> Failure(string error) => new(default, error, false);

    /// <summary>
    /// Indicates whether the operation succeeded
    /// </summary>
    public bool IsSuccess => _isSuccess;

    /// <summary>
    /// Indicates whether the operation failed
    /// </summary>
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// Gets the value if successful, otherwise returns the default value
    /// </summary>
    public T? Value => _value;

    /// <summary>
    /// Gets the error message if failed
    /// </summary>
    public string? Error => _error;

    /// <summary>
    /// Maps the success value to a new type using a pure function
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper) =>
        _isSuccess && _value != null
            ? Result<TNew>.Success(mapper(_value))
            : Result<TNew>.Failure(_error ?? "Unknown error");

    /// <summary>
    /// Binds (flatMaps) the result to another operation that returns a Result
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder) =>
        _isSuccess && _value != null
            ? binder(_value)
            : Result<TNew>.Failure(_error ?? "Unknown error");

    /// <summary>
    /// Executes an action if the result is successful
    /// </summary>
    public Result<T> OnSuccess(Action<T> action)
    {
        if (_isSuccess && _value != null)
            action(_value);
        return this;
    }

    /// <summary>
    /// Executes an action if the result failed
    /// </summary>
    public Result<T> OnFailure(Action<string> action)
    {
        if (!_isSuccess && _error != null)
            action(_error);
        return this;
    }

    /// <summary>
    /// Returns the value if successful, otherwise returns the provided default value
    /// </summary>
    public T GetOrDefault(T defaultValue) =>
        _isSuccess && _value != null ? _value : defaultValue;

    /// <summary>
    /// Returns the value if successful, otherwise throws an exception
    /// </summary>
    public T GetOrThrow() =>
        _isSuccess && _value != null
            ? _value
            : throw new InvalidOperationException(_error ?? "Operation failed");

    /// <summary>
    /// Pattern matching for functional composition
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure) =>
        _isSuccess && _value != null
            ? onSuccess(_value)
            : onFailure(_error ?? "Unknown error");
}

/// <summary>
/// Extension methods for working with Result types
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a Task of Result to support async composition
    /// </summary>
    public static async Task<Result<TNew>> MapAsync<T, TNew>(
        this Task<Result<T>> resultTask,
        Func<T, TNew> mapper)
    {
        var result = await resultTask;
        return result.Map(mapper);
    }

    /// <summary>
    /// Binds async operations on Result
    /// </summary>
    public static async Task<Result<TNew>> BindAsync<T, TNew>(
        this Task<Result<T>> resultTask,
        Func<T, Task<Result<TNew>>> binder)
    {
        var result = await resultTask;
        return result.IsSuccess && result.Value != null
            ? await binder(result.Value)
            : Result<TNew>.Failure(result.Error ?? "Unknown error");
    }

    /// <summary>
    /// Combines multiple results into a single result containing a collection
    /// </summary>
    public static Result<IEnumerable<T>> Sequence<T>(this IEnumerable<Result<T>> results)
    {
        var resultList = results.ToList();
        var failures = resultList.Where(r => r.IsFailure).ToList();

        if (failures.Any())
        {
            var errors = string.Join("; ", failures.Select(f => f.Error));
            return Result<IEnumerable<T>>.Failure(errors);
        }

        var values = resultList.Select(r => r.Value!);
        return Result<IEnumerable<T>>.Success(values);
    }

    /// <summary>
    /// Executes a side effect (I/O operation) only if the result is successful
    /// </summary>
    public static async Task<Result<T>> TapAsync<T>(
        this Result<T> result,
        Func<T, Task> asyncAction)
    {
        if (result.IsSuccess && result.Value != null)
            await asyncAction(result.Value);
        return result;
    }
}
