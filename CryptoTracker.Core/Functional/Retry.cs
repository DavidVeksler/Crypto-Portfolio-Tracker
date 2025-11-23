namespace CryptoTracker.Core.Functional;

/// <summary>
/// Functional retry utilities for composable error handling.
/// All functions are pure with explicit error handling through Result types.
/// </summary>
public static class Retry
{
    /// <summary>
    /// Tries multiple async operations in sequence until one succeeds.
    /// Returns the first successful result or accumulates all failures.
    /// </summary>
    public static async Task<Result<T>> FirstSuccess<T>(
        IEnumerable<Func<Task<Result<T>>>> operations)
    {
        var errors = new List<string>();

        foreach (var operation in operations)
        {
            var result = await operation();
            if (result.IsSuccess)
                return result;

            if (result.Error != null)
                errors.Add(result.Error);
        }

        var combinedError = string.Join("; ", errors);
        return Result<T>.Failure($"All operations failed: {combinedError}");
    }

    /// <summary>
    /// Tries multiple async operations in sequence until one succeeds, with logging.
    /// Returns the first successful result or accumulates all failures.
    /// </summary>
    public static async Task<Result<T>> FirstSuccessWithLog<T>(
        IEnumerable<Func<Task<Result<T>>>> operations,
        Action<string> onAttempt,
        Action<string> onFailure)
    {
        var errors = new List<string>();

        foreach (var operation in operations)
        {
            try
            {
                var result = await operation();
                if (result.IsSuccess)
                {
                    onAttempt("Operation succeeded");
                    return result;
                }

                if (result.Error != null)
                {
                    errors.Add(result.Error);
                    onFailure(result.Error);
                }
            }
            catch (Exception ex)
            {
                var error = $"Exception: {ex.Message}";
                errors.Add(error);
                onFailure(error);
            }
        }

        var combinedError = string.Join("; ", errors);
        return Result<T>.Failure($"All operations failed: {combinedError}");
    }

    /// <summary>
    /// Retries an operation a specified number of times with exponential backoff.
    /// </summary>
    public static async Task<Result<T>> WithExponentialBackoff<T>(
        Func<Task<Result<T>>> operation,
        int maxAttempts,
        TimeSpan initialDelay)
    {
        var errors = new List<string>();
        var delay = initialDelay;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var result = await operation();
            if (result.IsSuccess)
                return result;

            if (result.Error != null)
                errors.Add($"Attempt {attempt}: {result.Error}");

            if (attempt < maxAttempts)
                await Task.Delay(delay);

            delay *= 2; // Exponential backoff
        }

        var combinedError = string.Join("; ", errors);
        return Result<T>.Failure($"Failed after {maxAttempts} attempts: {combinedError}");
    }

    /// <summary>
    /// Creates a sequence of retry operations from a list of items.
    /// Each item is transformed into an operation that can be tried.
    /// </summary>
    public static IEnumerable<Func<Task<Result<T>>>> CreateRetrySequence<TSource, T>(
        IEnumerable<TSource> items,
        Func<TSource, Task<Result<T>>> operation) =>
        items.Select<TSource, Func<Task<Result<T>>>>(item => async () => await operation(item));

    /// <summary>
    /// Wraps an operation that may throw exceptions into a Result.
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            var result = await operation();
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Wraps a synchronous operation that may throw exceptions into a Result.
    /// </summary>
    public static Result<T> Try<T>(Func<T> operation)
    {
        try
        {
            var result = operation();
            return Result<T>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(ex.Message);
        }
    }
}
