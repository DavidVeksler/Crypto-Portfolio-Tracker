namespace CryptoTracker.Core.Functional;

/// <summary>
/// Functional utilities for working with async sequences and streams.
/// All functions are pure and composable.
/// </summary>
public static class AsyncSequence
{
    /// <summary>
    /// Creates an infinite async sequence starting from a seed value
    /// </summary>
    public static async IAsyncEnumerable<T> Unfold<T>(T seed, Func<T, Task<T>> generator)
    {
        var current = seed;
        while (true)
        {
            yield return current;
            current = await generator(current);
        }
    }

    /// <summary>
    /// Creates an infinite async sequence with index and state
    /// </summary>
    public static async IAsyncEnumerable<TResult> UnfoldIndexed<TState, TResult>(
        TState initialState,
        Func<int, TState, Task<(TResult result, TState newState)>> generator)
    {
        var state = initialState;
        var index = 0;
        while (true)
        {
            var (result, newState) = await generator(index, state);
            yield return result;
            state = newState;
            index++;
        }
    }

    /// <summary>
    /// Takes elements from an async sequence until a predicate is false
    /// </summary>
    public static async IAsyncEnumerable<T> TakeWhile<T>(
        this IAsyncEnumerable<T> source,
        Func<T, bool> predicate)
    {
        await foreach (var item in source)
        {
            if (!predicate(item))
                yield break;
            yield return item;
        }
    }

    /// <summary>
    /// Takes elements from an async sequence until a predicate becomes true
    /// </summary>
    public static async IAsyncEnumerable<T> TakeUntil<T>(
        this IAsyncEnumerable<T> source,
        Func<T, bool> predicate)
    {
        await foreach (var item in source)
        {
            yield return item;
            if (predicate(item))
                yield break;
        }
    }

    /// <summary>
    /// Applies a transformation to each element in an async sequence
    /// </summary>
    public static async IAsyncEnumerable<TResult> Select<T, TResult>(
        this IAsyncEnumerable<T> source,
        Func<T, TResult> selector)
    {
        await foreach (var item in source)
        {
            yield return selector(item);
        }
    }

    /// <summary>
    /// Applies an async transformation to each element in an async sequence
    /// </summary>
    public static async IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
        this IAsyncEnumerable<T> source,
        Func<T, Task<TResult>> selector)
    {
        await foreach (var item in source)
        {
            yield return await selector(item);
        }
    }

    /// <summary>
    /// Scans an async sequence with an accumulator function (like fold but returns intermediate results)
    /// </summary>
    public static async IAsyncEnumerable<TAccumulate> Scan<T, TAccumulate>(
        this IAsyncEnumerable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T, TAccumulate> accumulator)
    {
        var accumulated = seed;
        await foreach (var item in source)
        {
            accumulated = accumulator(accumulated, item);
            yield return accumulated;
        }
    }

    /// <summary>
    /// Scans an async sequence with an async accumulator function
    /// </summary>
    public static async IAsyncEnumerable<TAccumulate> ScanAsync<T, TAccumulate>(
        this IAsyncEnumerable<T> source,
        TAccumulate seed,
        Func<TAccumulate, T, Task<TAccumulate>> accumulator)
    {
        var accumulated = seed;
        await foreach (var item in source)
        {
            accumulated = await accumulator(accumulated, item);
            yield return accumulated;
        }
    }

    /// <summary>
    /// Materializes an async sequence to a list
    /// </summary>
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }

    /// <summary>
    /// Finds the last element in an async sequence that matches a predicate
    /// </summary>
    public static async Task<Option<T>> LastOrNone<T>(
        this IAsyncEnumerable<T> source,
        Func<T, bool> predicate)
    {
        var last = Option<T>.None();
        await foreach (var item in source)
        {
            if (predicate(item))
                last = Option<T>.Some(item);
        }
        return last;
    }

    /// <summary>
    /// Creates a range of integers as an async sequence
    /// </summary>
    public static async IAsyncEnumerable<int> Range(int start, int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return start + i;
        }
    }

    /// <summary>
    /// Creates an infinite countdown sequence
    /// </summary>
    public static async IAsyncEnumerable<int> Countdown(int start, TimeSpan interval)
    {
        for (var i = start; i > 0; i--)
        {
            yield return i;
            await Task.Delay(interval);
        }
    }
}
