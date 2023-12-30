// See https://aka.ms/new-console-template for more information


using Console;
using Console.Bitcoin;
using Console.Services;


var service = new CoinGeckoService();

while (true)
{
    var info = service.GetCurrencyInfoAsync("usd", Settings.PricesToCheck).Result;
    TrackerConsolerRenderer.RenderCryptoPrices(info);

    var bitcoinPrice = info.Where(r=> r.Name=="Bitcoin").First().CurrentPrice;

    foreach (var key in Settings.XPubKeys)
    {
        var client = new ElectrumClient();
        var valueOfWallet = client.GetWalletBalanceAsync(key.Xpub, key.ScriptPubKeyType).Result;

        System.Console.WriteLine($"Wallet ({key.Xpub}) Value:");
        System.Console.WriteLine($"    In BTC: {valueOfWallet:N8}"); // 'N8' formats number with 8 decimal places
        System.Console.WriteLine($"    In USD: {(valueOfWallet * bitcoinPrice):C2}"); // 'C2' formats as currency with 2 decimal places
    }

    // Start a 30-second countdown
    for (int i = 30; i > 0; i--)
    {
        System.Console.SetCursorPosition(0, System.Console.CursorTop);
        System.Console.Write($"Refreshing in {i} seconds... ");
        await Task.Delay(1000); // Wait for 1 second
    }
    

    System.Console.Clear(); // Clear the console for the next update
}


