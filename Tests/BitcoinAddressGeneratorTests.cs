using CryptoTracker.Core.Services.Bitcoin;
using NBitcoin;

namespace Tests;

[TestFixture]
public class BitcoinAddressGeneratorTests
{
    private const string TestXpub = "xpub661MyMwAqRbcFtXgS5sYJABqqG9YLmC4Q1Rdap9gSE8NqtwybGhePY2gZ29ESFjqJoCu1Rupje8YtGqsefD265TMg7usUDFdp6W1EGMcet8";
    private const string TestZpub = "zpub6rFR7y4Q2AijBEqTUquhVz398htDFrtymD9xYYfG1m4wAcvphXR5ePCqYAN5qRbNnCLan84456drkkGDbz7zqPPrKJj9yKPGpCTAwaTj5bx";

    [Test]
    public void GeneratesValidLegacyAddress()
    {
        var generator = new BitcoinAddressGenerator(TestXpub);
        var address = generator.GenerateAddress(0, ScriptPubKeyType.Legacy);

        Assert.That(address, Is.Not.Null);
        Assert.That(address, Is.Not.Empty);
        Assert.That(address, Does.StartWith("1"));
    }

    [Test]
    public void GeneratesValidSegwitAddress()
    {
        var generator = new BitcoinAddressGenerator(TestXpub);
        var address = generator.GenerateAddress(0, ScriptPubKeyType.Segwit);

        Assert.That(address, Is.Not.Null);
        Assert.That(address, Is.Not.Empty);
        Assert.That(address, Does.StartWith("bc1"));
    }

    [Test]
    public void GeneratesValidSegwitP2SHAddress()
    {
        var generator = new BitcoinAddressGenerator(TestXpub);
        var address = generator.GenerateAddress(0, ScriptPubKeyType.SegwitP2SH);

        Assert.That(address, Is.Not.Null);
        Assert.That(address, Is.Not.Empty);
        Assert.That(address, Does.StartWith("3"));
    }

    [Test]
    public void GeneratesDifferentAddressesForDifferentIndices()
    {
        var generator = new BitcoinAddressGenerator(TestXpub);
        var address0 = generator.GenerateAddress(0, ScriptPubKeyType.Legacy);
        var address1 = generator.GenerateAddress(1, ScriptPubKeyType.Legacy);
        var address2 = generator.GenerateAddress(2, ScriptPubKeyType.Legacy);

        Assert.That(address0, Is.Not.EqualTo(address1));
        Assert.That(address1, Is.Not.EqualTo(address2));
        Assert.That(address0, Is.Not.EqualTo(address2));
    }

    [Test]
    public void GeneratesSameAddressForSameIndex()
    {
        var generator = new BitcoinAddressGenerator(TestXpub);
        var address1 = generator.GenerateAddress(5, ScriptPubKeyType.Legacy);
        var address2 = generator.GenerateAddress(5, ScriptPubKeyType.Legacy);

        Assert.That(address1, Is.EqualTo(address2));
    }

    [Test]
    public void ThrowsOnNegativeIndex()
    {
        var generator = new BitcoinAddressGenerator(TestXpub);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            generator.GenerateAddress(-1, ScriptPubKeyType.Legacy));
    }

    [Test]
    public void ThrowsOnInvalidZpubKey()
    {
        // Invalid zpub key should throw FormatException
        Assert.Throws<FormatException>(() =>
            new BitcoinAddressGenerator(TestZpub));
    }

    [Test]
    public void GeneratesValidAddressesForLargeIndices()
    {
        var generator = new BitcoinAddressGenerator(TestXpub);
        var address = generator.GenerateAddress(1000, ScriptPubKeyType.Legacy);

        Assert.That(address, Is.Not.Null);
        Assert.That(address, Is.Not.Empty);
        Assert.That(address, Does.StartWith("1"));
    }

    [Test]
    public void ThrowsOnInvalidXpubKey()
    {
        Assert.Throws<FormatException>(() =>
            new BitcoinAddressGenerator("invalid_xpub_key"));
    }

    [Test]
    public void DifferentScriptTypesGenerateDifferentAddresses()
    {
        var generator = new BitcoinAddressGenerator(TestXpub);
        var legacyAddress = generator.GenerateAddress(0, ScriptPubKeyType.Legacy);
        var segwitAddress = generator.GenerateAddress(0, ScriptPubKeyType.Segwit);
        var segwitP2SHAddress = generator.GenerateAddress(0, ScriptPubKeyType.SegwitP2SH);

        Assert.That(legacyAddress, Is.Not.EqualTo(segwitAddress));
        Assert.That(segwitAddress, Is.Not.EqualTo(segwitP2SHAddress));
        Assert.That(legacyAddress, Is.Not.EqualTo(segwitP2SHAddress));
    }

    [Test]
    public void GeneratesMultipleAddressesSuccessively()
    {
        var generator = new BitcoinAddressGenerator(TestXpub);
        var addresses = new List<string>();

        for (int i = 0; i < 10; i++)
        {
            addresses.Add(generator.GenerateAddress(i, ScriptPubKeyType.Legacy));
        }

        Assert.That(addresses, Has.Count.EqualTo(10));
        Assert.That(addresses.Distinct().Count(), Is.EqualTo(10)); // All unique
    }
}
