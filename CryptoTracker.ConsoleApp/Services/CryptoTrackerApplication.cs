using CryptoTracker.Core.Abstractions;
using CryptoTracker.Core.Configuration;
using CryptoTracker.Core.Constants;
using CryptoTracker.ConsoleApp.CoinStatusRenderingService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoTracker.ConsoleApp.Services;

/// <summary>
/// Main application orchestrator for the crypto portfolio tracker.
/// </summary>
public class CryptoTrackerApplication
{
    private readonly ICryptoPriceService _priceService;
    private readonly IBatchEthereumBalanceService _ethereumBalanceService;
    private readonly IWalletTracker _walletTracker;
    private readonly CryptoTrackerOptions _options;
    private readonly IConsoleRenderer _consoleRenderer;
    private readonly ILogger<CryptoTrackerApplication> _logger;

    public CryptoTrackerApplication(
        ICryptoPriceService priceService,
        IBatchEthereumBalanceService ethereumBalanceService,
        IWalletTracker walletTracker,
        IOptions<CryptoTrackerOptions> options,
        IConsoleRenderer consoleRenderer,
        ILogger<CryptoTrackerApplication> logger)
    {
        _priceService = priceService;
        _ethereumBalanceService = ethereumBalanceService;
        _walletTracker = walletTracker;
        _options = options.Value;
        _consoleRenderer = consoleRenderer;
        _logger = logger;
    }

    /// <summary>
    /// Runs the main application loop.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Crypto Portfolio Tracker...");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await UpdateAndDisplayAsync();
                await RenderRefreshTimerAsync(AppConstants.UI.RefreshIntervalSeconds, cancellationToken);
                Console.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during update cycle");
                await Task.Delay(5000, cancellationToken); // Wait before retry
            }
        }

        _logger.LogInformation("Crypto Portfolio Tracker stopped.");
    }

    private async Task UpdateAndDisplayAsync()
    {
        // Fetch and render cryptocurrency prices
        var cryptoInfo = await _priceService.GetCurrencyInfoAsync("usd", _options.PricesToCheck);
        _consoleRenderer.RenderCryptoPrices(cryptoInfo);

        // Display Ethereum balances if configured
        if (_options.Ethereum.AddressesToMonitor.Any())
        {
            await DisplayEthereumBalancesAsync(cryptoInfo);
        }

        // Display Bitcoin wallet balances if configured
        if (_options.XpubKeyPairs.Any(key => key.Xpub.Length > 4))
        {
            await DisplayBitcoinBalancesAsync(cryptoInfo);
        }
    }

    private async Task DisplayEthereumBalancesAsync(Core.Services.CryptoPriceServices.CoinInfo[] cryptoInfo)
    {
        var balances = await _ethereumBalanceService.GetEthereumBalanceAsync(
            _options.Ethereum.AddressesToMonitor.ToArray());

        Console.WriteLine("\n--- Ethereum Balances ---");

        foreach (var (address, balance) in _options.Ethereum.AddressesToMonitor.Zip(balances))
        {
            Console.WriteLine($"Ethereum balance for {address}: {balance}");
        }

        var ethereumPrice = cryptoInfo.FirstOrDefault(r => r.Name == "Ethereum")?.CurrentPrice ?? 0;
        var totalUsdValue = balances.Sum(decimal.Parse) * ethereumPrice;
        Console.WriteLine($"    Total in USD: {totalUsdValue:C2}");
    }

    private async Task DisplayBitcoinBalancesAsync(Core.Services.CryptoPriceServices.CoinInfo[] cryptoInfo)
    {
        var bitcoinPrice = cryptoInfo.FirstOrDefault(r => r.Name == "Bitcoin")?.CurrentPrice ?? 0;
        Console.WriteLine("\n--- Bitcoin Wallet Values ---");

        foreach (var key in _options.XpubKeyPairs.Where(key => key.Xpub.Length > 4))
        {
            var valueOfWallet = await _walletTracker.GetWalletBalanceAsync(key.Xpub, key.ScriptPubKeyType);

            Console.WriteLine($"\nWallet ({key.Xpub[..20]}...) Value:");
            Console.WriteLine($"    In BTC: {valueOfWallet:N8}");
            Console.WriteLine($"    In USD: {valueOfWallet * bitcoinPrice:C2}");
        }
    }

    private static async Task RenderRefreshTimerAsync(int seconds, CancellationToken cancellationToken)
    {
        for (var i = seconds; i > 0; i--)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Refreshing in {i} seconds... ");
            await Task.Delay(1000, cancellationToken);
        }
    }
}
