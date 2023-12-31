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
            var gen = new BitcoinAddressGenerator("");
            var address = gen.LegacyGen("23c9686722351fa8b0abce4078641ca879b6978694a1118a53027cf30c25322b6f23fc94c0a41e8af837343a6f400a118b7605bbd4c5dc4063db9c99abed5f9f");

            Assert.Pass();
        }
    }
}