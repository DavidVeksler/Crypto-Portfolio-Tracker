using NBitcoin;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            //var gen = new BitcoinAddressGenerator("");
            
            var key = new BitcoinExtPubKey("",Network.Main);

            var address = key.Derive(new KeyPath("m")).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            

            Assert.Pass();
        }
    }
}