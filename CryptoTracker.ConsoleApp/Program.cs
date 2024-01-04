// See https://aka.ms/new-console-template for more information

using Console.Services;
using CryptoTracker.ConsoleApp.CoinStatusRenderingService;
using CryptoTracker.Core.Infrastructure.Configuration;
using CryptoTracker.Core.Services.Electrum;

CoinGeckoService service = new();

while (true)
{
    CoinInfo[] info = service.GetCurrencyInfoAsync("usd", ConfigSettings.PricesToCheck).Result;
    TrackerConsolerRenderer.RenderCryptoPrices(info);

    if (ConfigSettings.EthereumAddressesToMonitor.Count() > 0)
    {
        var balances = EtherscanBalanceChecker.GetEthereumBalanceAsync(ConfigSettings.EthereumAddressesToMonitor.ToArray()).Result;

        for (int i = 0; i < balances.Count(); i++)
        {
            System.Console.WriteLine($"Ethereum balance for {ConfigSettings.EthereumAddressesToMonitor.ToList()[i]} : {balances[i]}");
        }
    }

    if (ConfigSettings.XPubKeys.Count() > 0)
    {

        decimal bitcoinPrice = info.Where(r => r.Name == "Bitcoin").First().CurrentPrice;

        foreach (XpubKeyPair key in ConfigSettings.XPubKeys.Where(key=> key.Xpub.Length > 4))
        {
            ElectrumCryptoWalletTracker client = new();
            decimal valueOfWallet = client.GetWalletBalanceAsync(key.Xpub, key.ScriptPubKeyType).Result;

            System.Console.WriteLine($"Wallet ({key.Xpub}) Value:");
            System.Console.WriteLine($"    In BTC: {valueOfWallet:N8}"); // 'N8' formats number with 8 decimal places
            System.Console.WriteLine($"    In USD: {valueOfWallet * bitcoinPrice:C2}"); // 'C2' formats as currency with 2 decimal places
        }

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