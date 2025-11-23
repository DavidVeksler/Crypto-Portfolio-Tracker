using CryptoTracker.Core.Functional;

namespace Tests;

[TestFixture]
public class OptionTests
{
    [Test]
    public void Some_CreatesOptionWithValue()
    {
        var option = Option<int>.Some(42);

        Assert.That(option.IsSome, Is.True);
        Assert.That(option.IsNone, Is.False);
        Assert.That(option.Value, Is.EqualTo(42));
    }

    [Test]
    public void None_CreatesEmptyOption()
    {
        var option = Option<int>.None();

        Assert.That(option.IsSome, Is.False);
        Assert.That(option.IsNone, Is.True);
    }

    [Test]
    public void FromNullable_CreatesNoneForNull()
    {
        string? nullString = null;
        var option = Option<string>.FromNullable(nullString);

        Assert.That(option.IsNone, Is.True);
    }

    [Test]
    public void FromNullable_CreatesSomeForNonNull()
    {
        var option = Option<string>.FromNullable("test");

        Assert.That(option.IsSome, Is.True);
        Assert.That(option.Value, Is.EqualTo("test"));
    }

    [Test]
    public void Map_TransformsValue()
    {
        var option = Option<int>.Some(5);
        var mapped = option.Map(x => x * 2);

        Assert.That(mapped.IsSome, Is.True);
        Assert.That(mapped.Value, Is.EqualTo(10));
    }

    [Test]
    public void Map_PropagatesNone()
    {
        var option = Option<int>.None();
        var mapped = option.Map(x => x * 2);

        Assert.That(mapped.IsNone, Is.True);
    }

    [Test]
    public void Bind_ChainsOperations()
    {
        var option = Option<int>.Some(10);
        var bound = option.Bind(x => Option<string>.Some($"Value: {x}"));

        Assert.That(bound.IsSome, Is.True);
        Assert.That(bound.Value, Is.EqualTo("Value: 10"));
    }

    [Test]
    public void Bind_StopsOnNone()
    {
        var option = Option<int>.None();
        var bound = option.Bind(x => Option<string>.Some($"Value: {x}"));

        Assert.That(bound.IsNone, Is.True);
    }

    [Test]
    public void Match_CallsCorrectBranch()
    {
        var some = Option<int>.Some(42);
        var none = Option<int>.None();

        var someResult = some.Match(
            onSome: x => $"Value: {x}",
            onNone: () => "No value");

        var noneResult = none.Match(
            onSome: x => $"Value: {x}",
            onNone: () => "No value");

        Assert.That(someResult, Is.EqualTo("Value: 42"));
        Assert.That(noneResult, Is.EqualTo("No value"));
    }

    [Test]
    public void Filter_KeepsValueIfPredicateTrue()
    {
        var option = Option<int>.Some(10);
        var filtered = option.Filter(x => x > 5);

        Assert.That(filtered.IsSome, Is.True);
        Assert.That(filtered.Value, Is.EqualTo(10));
    }

    [Test]
    public void Filter_BecomesNoneIfPredicateFalse()
    {
        var option = Option<int>.Some(3);
        var filtered = option.Filter(x => x > 5);

        Assert.That(filtered.IsNone, Is.True);
    }

    [Test]
    public void GetOrDefault_ReturnsValueOnSome()
    {
        var option = Option<int>.Some(42);
        Assert.That(option.GetOrDefault(0), Is.EqualTo(42));
    }

    [Test]
    public void GetOrDefault_ReturnsDefaultOnNone()
    {
        var option = Option<int>.None();
        Assert.That(option.GetOrDefault(99), Is.EqualTo(99));
    }

    [Test]
    public void GetOrElse_ReturnsValueOnSome()
    {
        var option = Option<int>.Some(42);
        Assert.That(option.GetOrElse(() => 0), Is.EqualTo(42));
    }

    [Test]
    public void GetOrElse_ComputesDefaultOnNone()
    {
        var option = Option<int>.None();
        Assert.That(option.GetOrElse(() => 100), Is.EqualTo(100));
    }

    [Test]
    public void GetOrThrow_ReturnsValueOnSome()
    {
        var option = Option<int>.Some(42);
        Assert.That(option.GetOrThrow(), Is.EqualTo(42));
    }

    [Test]
    public void GetOrThrow_ThrowsOnNone()
    {
        var option = Option<int>.None();
        Assert.Throws<InvalidOperationException>(() => option.GetOrThrow());
    }

    [Test]
    public void OnSome_ExecutesActionOnSome()
    {
        var executed = false;
        var option = Option<int>.Some(42);

        option.OnSome(x => executed = true);

        Assert.That(executed, Is.True);
    }

    [Test]
    public void OnSome_DoesNotExecuteOnNone()
    {
        var executed = false;
        var option = Option<int>.None();

        option.OnSome(x => executed = true);

        Assert.That(executed, Is.False);
    }

    [Test]
    public void OnNone_ExecutesActionOnNone()
    {
        var executed = false;
        var option = Option<int>.None();

        option.OnNone(() => executed = true);

        Assert.That(executed, Is.True);
    }

    [Test]
    public void OnNone_DoesNotExecuteOnSome()
    {
        var executed = false;
        var option = Option<int>.Some(42);

        option.OnNone(() => executed = true);

        Assert.That(executed, Is.False);
    }

    [Test]
    public void ToResult_ConvertsSuccessfully()
    {
        var some = Option<int>.Some(42);
        var none = Option<int>.None();

        var someResult = some.ToResult("No value");
        var noneResult = none.ToResult("Value missing");

        Assert.That(someResult.IsSuccess, Is.True);
        Assert.That(someResult.Value, Is.EqualTo(42));
        Assert.That(noneResult.IsFailure, Is.True);
        Assert.That(noneResult.Error, Is.EqualTo("Value missing"));
    }

    [Test]
    public void Choose_FiltersNoneValues()
    {
        var options = new[]
        {
            Option<int>.Some(1),
            Option<int>.None(),
            Option<int>.Some(3),
            Option<int>.None(),
            Option<int>.Some(5)
        };

        var values = options.Choose().ToList();

        Assert.That(values, Is.EquivalentTo(new[] { 1, 3, 5 }));
    }

    [Test]
    public void FirstOrNone_ReturnsFirstElement()
    {
        var items = new[] { 1, 2, 3 };
        var option = items.FirstOrNone();

        Assert.That(option.IsSome, Is.True);
        Assert.That(option.Value, Is.EqualTo(1));
    }

    [Test]
    public void FirstOrNone_ReturnsNoneForEmptySequence()
    {
        var items = Array.Empty<int>();
        var option = items.FirstOrNone();

        Assert.That(option.IsNone, Is.True);
    }

    [Test]
    public void FirstOrNone_WithPredicate_FindsMatch()
    {
        var items = new[] { 1, 2, 3, 4, 5 };
        var option = items.FirstOrNone(x => x > 3);

        Assert.That(option.IsSome, Is.True);
        Assert.That(option.Value, Is.EqualTo(4));
    }

    [Test]
    public void FirstOrNone_WithPredicate_ReturnsNoneIfNoMatch()
    {
        var items = new[] { 1, 2, 3 };
        var option = items.FirstOrNone(x => x > 10);

        Assert.That(option.IsNone, Is.True);
    }
}
