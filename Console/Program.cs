// See https://aka.ms/new-console-template for more information


//TrackerConsolerRenderer.RenderCryptoTickerAsync();


var key = Settings.XPubKeys.First();
var address = new BitcoinAddressGenerator(key.Xpub).GetBitcoinAddress(0, key.ScriptPubKeyType);
System.Console.WriteLine(address);



