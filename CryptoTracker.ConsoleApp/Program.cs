// Other using statements...

using Console.Services;
using CryptoTracker.ConsoleApp.CoinStatusRenderingService;
using CryptoTracker.Core.Infrastructure.Configuration;
using CryptoTracker.Core.Services.Electrum;

// ...

CoinGeckoService service = new();
var client = new ElectrumCryptoWalletTracker();

while (true)
{
    var info = await service.GetCurrencyInfoAsync("usd", ConfigSettings.PricesToCheck);
    TrackerConsolerRenderer.RenderCryptoPrices(info);

    if (ConfigSettings.EthereumAddressesToMonitor.Any())
    {
        var balances =
            await EtherscanBalanceChecker.GetEthereumBalanceAsync(ConfigSettings.EthereumAddressesToMonitor.ToArray());
        System.Console.WriteLine("\n--- Ethereum Balances ---");
        foreach (var (address, balance) in ConfigSettings.EthereumAddressesToMonitor.Zip(balances))
            System.Console.WriteLine($"Ethereum balance for {address}: {balance}");

        var ethereumPrice = info.FirstOrDefault(r => r.Name == "Ethereum")?.CurrentPrice ?? 0;
        System.Console.WriteLine($"    In USD: {balances.Sum(decimal.Parse) * ethereumPrice:C2}");
    }

    if (ConfigSettings.XPubKeys.Any(key => key.Xpub.Length > 4))
    {
        var bitcoinPrice = info.FirstOrDefault(r => r.Name == "Bitcoin")?.CurrentPrice ?? 0;
        System.Console.WriteLine("\n--- Bitcoin Wallet Values ---");
        foreach (var key in ConfigSettings.XPubKeys.Where(key => key.Xpub.Length > 4))
        {
            var valueOfWallet = await client.GetWalletBalanceAsync(key.Xpub, key.ScriptPubKeyType);

            System.Console.WriteLine($"\nWallet ({key.Xpub}) Value:");
            System.Console.WriteLine($"    In BTC: {valueOfWallet:N8}");
            System.Console.WriteLine($"    In USD: {valueOfWallet * bitcoinPrice:C2}");
        }
    }

    // Refresh timer
    await RenderRefreshTimer(30);
    System.Console.Clear();
}

async Task RenderRefreshTimer(int seconds)
{
    for (var i = seconds; i > 0; i--)
    {
        System.Console.SetCursorPosition(0, System.Console.CursorTop);
        System.Console.Write($"Refreshing in {i} seconds... ");
        await Task.Delay(1000);
    }
}