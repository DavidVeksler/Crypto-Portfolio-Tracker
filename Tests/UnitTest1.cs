using CryptoTracker.Core.Services.Bitcoin;
using NBitcoin;

namespace Tests;

[TestFixture]
public class CryptoTests
{
    [Test]
    public void BitcoinAddressGenerator_GeneratesValidAddress()
    {
        // Example xpub key for testing
        const string testXpub = "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8";

        var generator = new BitcoinAddressGenerator(testXpub);
        var address = generator.GenerateAddress(0, ScriptPubKeyType.Legacy);

        // Assert that the generated address is valid and not empty
        Assert.That(address, Is.Not.Null);
        Assert.That(address, Is.Not.Empty);
        Assert.That(address, Does.StartWith("1")); // Legacy addresses start with 1
    }

    [Test]
    public void BitcoinAddressGenerator_GeneratesSegwitAddress()
    {
        const string testXpub = "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8";

        var generator = new BitcoinAddressGenerator(testXpub);
        var address = generator.GenerateAddress(0, ScriptPubKeyType.Segwit);

        // Assert that the generated address is valid and is a bech32 address
        Assert.That(address, Is.Not.Null);
        Assert.That(address, Is.Not.Empty);
        Assert.That(address, Does.StartWith("bc1")); // Segwit addresses start with bc1
    }

    [Test]
    public void BitcoinAddressGenerator_ThrowsOnNegativeIndex()
    {
        const string testXpub = "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8";

        var generator = new BitcoinAddressGenerator(testXpub);

        // Assert that negative index throws ArgumentOutOfRangeException
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.GenerateAddress(-1, ScriptPubKeyType.Legacy));
    }
}