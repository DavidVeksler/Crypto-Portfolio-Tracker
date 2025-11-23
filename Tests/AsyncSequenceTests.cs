using CryptoTracker.Core.Functional;

namespace Tests;

[TestFixture]
public class AsyncSequenceTests
{
    [Test]
    public async Task TakeWhile_TakesElementsUntilPredicateFalse()
    {
        var sequence = AsyncSequence.Range(1, 10);
        var result = await sequence.TakeWhile(x => x <= 5).ToListAsync();

        Assert.That(result, Is.EquivalentTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [Test]
    public async Task TakeUntil_TakesElementsUntilPredicateTrue()
    {
        var sequence = AsyncSequence.Range(1, 10);
        var result = await sequence.TakeUntil(x => x == 5).ToListAsync();

        Assert.That(result, Is.EquivalentTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [Test]
    public async Task Select_TransformsElements()
    {
        var sequence = AsyncSequence.Range(1, 5);
        var result = await sequence.Select(x => x * 2).ToListAsync();

        Assert.That(result, Is.EquivalentTo(new[] { 2, 4, 6, 8, 10 }));
    }

    [Test]
    public async Task SelectAsync_TransformsElementsAsynchronously()
    {
        var sequence = AsyncSequence.Range(1, 3);
        var result = await sequence
            .SelectAsync(async x =>
            {
                await Task.Delay(1);
                return x * 3;
            })
            .ToListAsync();

        Assert.That(result, Is.EquivalentTo(new[] { 3, 6, 9 }));
    }

    [Test]
    public async Task Scan_AccumulatesValues()
    {
        var sequence = AsyncSequence.Range(1, 5);
        var result = await sequence.Scan(0, (acc, x) => acc + x).ToListAsync();

        Assert.That(result, Is.EquivalentTo(new[] { 1, 3, 6, 10, 15 }));
    }

    [Test]
    public async Task ScanAsync_AccumulatesValuesAsynchronously()
    {
        var sequence = AsyncSequence.Range(1, 4);
        var result = await sequence
            .ScanAsync(0, async (acc, x) =>
            {
                await Task.Delay(1);
                return acc + x;
            })
            .ToListAsync();

        Assert.That(result, Is.EquivalentTo(new[] { 1, 3, 6, 10 }));
    }

    [Test]
    public async Task LastOrNone_FindsLastMatchingElement()
    {
        var sequence = AsyncSequence.Range(1, 10);
        var result = await sequence.LastOrNone(x => x <= 10);

        Assert.That(result.IsSome, Is.True);
        Assert.That(result.Value, Is.EqualTo(10));
    }

    [Test]
    public async Task LastOrNone_ReturnsNoneIfNoMatch()
    {
        var sequence = AsyncSequence.Range(1, 5);
        var result = await sequence.LastOrNone(x => x > 100);

        Assert.That(result.IsNone, Is.True);
    }

    [Test]
    public async Task Range_GeneratesCorrectSequence()
    {
        var result = await AsyncSequence.Range(5, 3).ToListAsync();

        Assert.That(result, Is.EquivalentTo(new[] { 5, 6, 7 }));
    }

    [Test]
    public async Task Unfold_GeneratesInfiniteSequence()
    {
        var sequence = AsyncSequence.Unfold(1, async x =>
        {
            await Task.Delay(1);
            return x + 1;
        });

        var result = await sequence.TakeWhile(x => x <= 5).ToListAsync();

        Assert.That(result, Is.EquivalentTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [Test]
    public async Task UnfoldIndexed_GeneratesSequenceWithState()
    {
        var sequence = AsyncSequence.UnfoldIndexed(
            initialState: 0,
            generator: async (index, state) =>
            {
                await Task.Delay(1);
                var newState = state + index;
                return (result: newState, newState: newState);
            });

        var result = await sequence.TakeWhile(x => x < 10).ToListAsync();

        Assert.That(result, Is.EquivalentTo(new[] { 0, 1, 3, 6 }));
    }

    [Test]
    public async Task ToListAsync_MaterializesSequence()
    {
        var sequence = AsyncSequence.Range(1, 5);
        var result = await sequence.ToListAsync();

        Assert.That(result, Is.InstanceOf<List<int>>());
        Assert.That(result, Has.Count.EqualTo(5));
        Assert.That(result, Is.EquivalentTo(new[] { 1, 2, 3, 4, 5 }));
    }

    [Test]
    public async Task CombinedOperations_WorkCorrectly()
    {
        var result = await AsyncSequence.Range(1, 10)
            .Select(x => x * 2)
            .TakeWhile(x => x < 15)
            .Scan(0, (acc, x) => acc + x)
            .ToListAsync();

        Assert.That(result, Is.EquivalentTo(new[] { 2, 6, 12, 20, 30, 42, 56 }));
    }
}
