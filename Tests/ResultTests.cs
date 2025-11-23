using CryptoTracker.Core.Functional;

namespace Tests;

[TestFixture]
public class ResultTests
{
    [Test]
    public void Success_CreatesSuccessfulResult()
    {
        var result = Result<int>.Success(42);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(42));
        Assert.That(result.Error, Is.Null);
    }

    [Test]
    public void Failure_CreatesFailedResult()
    {
        var result = Result<int>.Failure("Something went wrong");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Something went wrong"));
    }

    [Test]
    public void Map_TransformsSuccessValue()
    {
        var result = Result<int>.Success(5);
        var mapped = result.Map(x => x * 2);

        Assert.That(mapped.IsSuccess, Is.True);
        Assert.That(mapped.Value, Is.EqualTo(10));
    }

    [Test]
    public void Map_PropagatesFailure()
    {
        var result = Result<int>.Failure("Error");
        var mapped = result.Map(x => x * 2);

        Assert.That(mapped.IsFailure, Is.True);
        Assert.That(mapped.Error, Is.EqualTo("Error"));
    }

    [Test]
    public void Bind_ChainsSuccessfulOperations()
    {
        var result = Result<int>.Success(10);
        var bound = result.Bind(x => Result<int>.Success(x / 2));

        Assert.That(bound.IsSuccess, Is.True);
        Assert.That(bound.Value, Is.EqualTo(5));
    }

    [Test]
    public void Bind_StopsOnFirstFailure()
    {
        var result = Result<int>.Success(10);
        var bound = result.Bind(x => Result<int>.Failure("Division error"));

        Assert.That(bound.IsFailure, Is.True);
        Assert.That(bound.Error, Is.EqualTo("Division error"));
    }

    [Test]
    public void Match_CallsCorrectBranch()
    {
        var success = Result<int>.Success(42);
        var failure = Result<int>.Failure("Error");

        var successResult = success.Match(
            onSuccess: x => $"Value: {x}",
            onFailure: e => $"Error: {e}");

        var failureResult = failure.Match(
            onSuccess: x => $"Value: {x}",
            onFailure: e => $"Error: {e}");

        Assert.That(successResult, Is.EqualTo("Value: 42"));
        Assert.That(failureResult, Is.EqualTo("Error: Error"));
    }

    [Test]
    public void GetOrDefault_ReturnsValueOnSuccess()
    {
        var result = Result<int>.Success(42);
        Assert.That(result.GetOrDefault(0), Is.EqualTo(42));
    }

    [Test]
    public void GetOrDefault_ReturnsDefaultOnFailure()
    {
        var result = Result<int>.Failure("Error");
        Assert.That(result.GetOrDefault(99), Is.EqualTo(99));
    }

    [Test]
    public void GetOrThrow_ReturnsValueOnSuccess()
    {
        var result = Result<int>.Success(42);
        Assert.That(result.GetOrThrow(), Is.EqualTo(42));
    }

    [Test]
    public void GetOrThrow_ThrowsOnFailure()
    {
        var result = Result<int>.Failure("Error");
        Assert.Throws<InvalidOperationException>(() => result.GetOrThrow());
    }

    [Test]
    public void OnSuccess_ExecutesActionOnSuccess()
    {
        var executed = false;
        var result = Result<int>.Success(42);

        result.OnSuccess(x => executed = true);

        Assert.That(executed, Is.True);
    }

    [Test]
    public void OnSuccess_DoesNotExecuteOnFailure()
    {
        var executed = false;
        var result = Result<int>.Failure("Error");

        result.OnSuccess(x => executed = true);

        Assert.That(executed, Is.False);
    }

    [Test]
    public void OnFailure_ExecutesActionOnFailure()
    {
        var executedError = "";
        var result = Result<int>.Failure("Test Error");

        result.OnFailure(e => executedError = e);

        Assert.That(executedError, Is.EqualTo("Test Error"));
    }

    [Test]
    public void OnFailure_DoesNotExecuteOnSuccess()
    {
        var executed = false;
        var result = Result<int>.Success(42);

        result.OnFailure(e => executed = true);

        Assert.That(executed, Is.False);
    }

    [Test]
    public async Task MapAsync_TransformsSuccessValue()
    {
        var resultTask = Task.FromResult(Result<int>.Success(5));
        var mapped = await resultTask.MapAsync(x => x * 3);

        Assert.That(mapped.IsSuccess, Is.True);
        Assert.That(mapped.Value, Is.EqualTo(15));
    }

    [Test]
    public async Task BindAsync_ChainsAsyncOperations()
    {
        var resultTask = Task.FromResult(Result<int>.Success(10));
        var bound = await resultTask.BindAsync(x =>
            Task.FromResult(Result<string>.Success($"Value: {x}")));

        Assert.That(bound.IsSuccess, Is.True);
        Assert.That(bound.Value, Is.EqualTo("Value: 10"));
    }

    [Test]
    public void Sequence_CombinesSuccessfulResults()
    {
        var results = new[]
        {
            Result<int>.Success(1),
            Result<int>.Success(2),
            Result<int>.Success(3)
        };

        var combined = results.Sequence();

        Assert.That(combined.IsSuccess, Is.True);
        Assert.That(combined.Value, Is.EquivalentTo(new[] { 1, 2, 3 }));
    }

    [Test]
    public void Sequence_FailsIfAnyResultFails()
    {
        var results = new[]
        {
            Result<int>.Success(1),
            Result<int>.Failure("Error in second"),
            Result<int>.Success(3)
        };

        var combined = results.Sequence();

        Assert.That(combined.IsFailure, Is.True);
        Assert.That(combined.Error, Does.Contain("Error in second"));
    }
}
