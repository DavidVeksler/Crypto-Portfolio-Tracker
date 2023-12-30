// See https://aka.ms/new-console-template for more information


//TrackerConsolerRenderer.RenderCryptoTickerAsync();


using Console.Bitcoin;

var key = Settings.XPubKeys[1];
var address = new BitcoinAddressGenerator(key.Xpub).GetBitcoinAddress(16, key.ScriptPubKeyType);


var client = new ElectrumClient();
var balance = client.GetBalanceAsync(address).Result;

System.Console.WriteLine(balance);



