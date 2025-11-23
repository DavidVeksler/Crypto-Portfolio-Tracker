using CryptoTracker.Core.Functional;

namespace Tests;

[TestFixture]
public class RetryTests
{
    [Test]
    public async Task FirstSuccess_ReturnsFirstSuccessfulOperation()
    {
        var operations = new List<Func<Task<Result<int>>>>
        {
            async () => { await Task.Delay(1); return Result<int>.Failure("First failed"); },
            async () => { await Task.Delay(1); return Result<int>.Success(42); },
            async () => { await Task.Delay(1); return Result<int>.Success(99); }
        };

        var result = await Retry.FirstSuccess(operations);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public async Task FirstSuccess_FailsIfAllOperationsFail()
    {
        var operations = new List<Func<Task<Result<int>>>>
        {
            async () => { await Task.Delay(1); return Result<int>.Failure("Error 1"); },
            async () => { await Task.Delay(1); return Result<int>.Failure("Error 2"); },
            async () => { await Task.Delay(1); return Result<int>.Failure("Error 3"); }
        };

        var result = await Retry.FirstSuccess(operations);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Error 1"));
        Assert.That(result.Error, Does.Contain("Error 2"));
        Assert.That(result.Error, Does.Contain("Error 3"));
    }

    [Test]
    public async Task WithExponentialBackoff_ReturnsSuccessOnFirstAttempt()
    {
        var attemptCount = 0;
        Func<Task<Result<int>>> operation = async () =>
        {
            attemptCount++;
            await Task.Delay(1);
            return Result<int>.Success(100);
        };

        var result = await Retry.WithExponentialBackoff(
            operation,
            maxAttempts: 3,
            initialDelay: TimeSpan.FromMilliseconds(10));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(100));
        Assert.That(attemptCount, Is.EqualTo(1));
    }

    [Test]
    public async Task WithExponentialBackoff_RetriesOnFailure()
    {
        var attemptCount = 0;
        Func<Task<Result<int>>> operation = async () =>
        {
            attemptCount++;
            await Task.Delay(1);
            return attemptCount < 3
                ? Result<int>.Failure($"Attempt {attemptCount} failed")
                : Result<int>.Success(42);
        };

        var result = await Retry.WithExponentialBackoff(
            operation,
            maxAttempts: 5,
            initialDelay: TimeSpan.FromMilliseconds(10));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(attemptCount, Is.EqualTo(3));
    }

    [Test]
    public async Task WithExponentialBackoff_FailsAfterMaxAttempts()
    {
        var attemptCount = 0;
        Func<Task<Result<int>>> operation = async () =>
        {
            attemptCount++;
            await Task.Delay(1);
            return Result<int>.Failure($"Attempt {attemptCount} failed");
        };

        var result = await Retry.WithExponentialBackoff(
            operation,
            maxAttempts: 3,
            initialDelay: TimeSpan.FromMilliseconds(10));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(attemptCount, Is.EqualTo(3));
        Assert.That(result.Error, Does.Contain("Failed after 3 attempts"));
    }

    [Test]
    public async Task TryAsync_WrapsSuccessfulOperation()
    {
        Func<Task<int>> operation = async () =>
        {
            await Task.Delay(1);
            return 42;
        };

        var result = await Retry.TryAsync(operation);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public async Task TryAsync_CatchesExceptions()
    {
        Func<Task<int>> operation = async () =>
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test exception");
        };

        var result = await Retry.TryAsync(operation);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Test exception"));
    }

    [Test]
    public void Try_WrapsSuccessfulOperation()
    {
        Func<int> operation = () => 42;

        var result = Retry.Try(operation);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(42));
    }

    [Test]
    public void Try_CatchesExceptions()
    {
        Func<int> operation = () => throw new InvalidOperationException("Test exception");

        var result = Retry.Try(operation);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Does.Contain("Test exception"));
    }

    [Test]
    public void CreateRetrySequence_CreatesCorrectSequence()
    {
        var items = new[] { 1, 2, 3 };
        Func<int, Task<Result<string>>> operation = async x =>
        {
            await Task.Delay(1);
            return Result<string>.Success($"Item {x}");
        };

        var sequence = Retry.CreateRetrySequence(items, operation).ToList();

        Assert.That(sequence, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task FirstSuccessWithLog_LogsAttempts()
    {
        var logs = new List<string>();
        var failures = new List<string>();

        var operations = new List<Func<Task<Result<int>>>>
        {
            async () => { await Task.Delay(1); return Result<int>.Failure("Error 1"); },
            async () => { await Task.Delay(1); return Result<int>.Success(42); }
        };

        var result = await Retry.FirstSuccessWithLog(
            operations,
            onAttempt: msg => logs.Add(msg),
            onFailure: msg => failures.Add(msg));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(failures, Has.Count.EqualTo(1));
        Assert.That(failures[0], Does.Contain("Error 1"));
        Assert.That(logs, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task FirstSuccessWithLog_CatchesExceptions()
    {
        var failures = new List<string>();

        var operations = new List<Func<Task<Result<int>>>>
        {
            async () =>
            {
                await Task.Delay(1);
                throw new Exception("Unexpected error");
            },
            async () => { await Task.Delay(1); return Result<int>.Success(42); }
        };

        var result = await Retry.FirstSuccessWithLog(
            operations,
            onAttempt: _ => { },
            onFailure: msg => failures.Add(msg));

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(failures, Has.Count.EqualTo(1));
        Assert.That(failures[0], Does.Contain("Exception"));
    }
}
