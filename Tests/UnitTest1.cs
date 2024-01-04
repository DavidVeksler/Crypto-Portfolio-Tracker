using Console.Ethereum;
using CryptoTracker.Core.Infrastructure.Configuration;
using NBitcoin;
using System.Diagnostics;

namespace Tests
{
    [TestFixture]
    public class CryptoTests
    {
        [SetUp]
        public void Initialize()
        {
            // Initialization code here, if any
        }

        [Test]
        public void BitcoinExtPubKey_GeneratesValidAddress()
        {
            var key = new BitcoinExtPubKey("", Network.Main);
            var address = key.Derive(new KeyPath("m")).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, Network.Main);

            // You should replace Assert.Pass() with an actual assertion that validates the result
            Assert.Pass();
        }

        [Test]
        public void EthereumBalanceLookup_ReturnsBalance_ForKnownAddress()
        {
            var balance = new InfuraBalanceLookupService().GetBalanceAsync(ConfigSettings.EthereumAddressesToMonitor.First()).Result;
            Debug.WriteLine(balance);

            // Add appropriate assertions to validate the balance
        }

        [Test]
        public void EtherscanBalanceLookup_ReturnsBalance_ForSampleEthereumAddress()
        {
            var balance = new InfuraBalanceLookupService().GetBalanceAsync(ConfigSettings.EthereumAddressesToMonitor.First()).Result;
            Debug.WriteLine(balance);

            // Add appropriate assertions to validate the balance
        }

        // Additional test methods as required
    }
}
