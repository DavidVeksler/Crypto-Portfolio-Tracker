// See https://aka.ms/new-console-template for more information


TrackerConsolerRenderer.RenderCryptoTickerAsync();


using Console.Bitcoin;

var key = Settings.XPubKeys[1];

var address = new BitcoinAddressGenerator(key.Xpub).GetBitcoinAddress(72, key.ScriptPubKeyType);
System.Console.WriteLine(address.ToString());   

var client = new ElectrumClient();

var valueOfWallet = client.GetWalletBalanceAsync(key.Xpub, key.ScriptPubKeyType).Result;

System.Console.WriteLine("BTC: " + valueOfWallet);
System.Console.WriteLine("Value In USD: " + valueOfWallet);
System.Console.ReadLine();

//var balance = ElectrumClient.SatoshiToBTC(client.GetBalanceAsync(address).Result.Result.Confirmed);

//System.Console.WriteLine(balance);



