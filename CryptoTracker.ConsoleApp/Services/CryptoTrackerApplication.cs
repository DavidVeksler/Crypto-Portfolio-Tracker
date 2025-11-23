using CryptoTracker.ConsoleApp.Abstractions;
using CryptoTracker.Core.Abstractions;
using CryptoTracker.Core.Configuration;
using CryptoTracker.Core.Constants;
using CryptoTracker.Core.Functional;
using CryptoTracker.Core.Services.CryptoPriceServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CryptoTracker.ConsoleApp.Services;

/// <summary>
/// Main application orchestrator for the crypto portfolio tracker.
/// </summary>
public class CryptoTrackerApplication
{
    private readonly ICryptoPriceService _priceService;
    private readonly IEthereumBalanceService _ethereumBalanceService;
    private readonly IWalletTracker _walletTracker;
    private readonly CryptoTrackerOptions _options;
    private readonly IConsoleRenderer _consoleRenderer;
    private readonly ILogger<CryptoTrackerApplication> _logger;

    public CryptoTrackerApplication(
        ICryptoPriceService priceService,
        IEthereumBalanceService ethereumBalanceService,
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
    /// Functional refactored version: Main loop as an async event stream.
    /// Uses IAsyncEnumerable for explicit data flow instead of while loop.
    /// </summary>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Crypto Portfolio Tracker...");

        // Create infinite refresh cycle stream
        var refreshCycles = CreateRefreshCycleStream(cancellationToken);

        // Process each cycle in the stream
        await foreach (var cycle in refreshCycles)
        {
            await ExecuteRefreshCycle(cycle, cancellationToken);
        }

        _logger.LogInformation("Crypto Portfolio Tracker stopped.");
    }

    /// <summary>
    /// Pure function: Creates an infinite stream of refresh cycle events.
    /// </summary>
    private static async IAsyncEnumerable<int> CreateRefreshCycleStream(CancellationToken cancellationToken)
    {
        var cycleNumber = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            yield return cycleNumber++;
        }
    }

    /// <summary>
    /// Executes a single refresh cycle with functional error handling.
    /// Isolates I/O side effects (console operations) to this boundary.
    /// </summary>
    private async Task ExecuteRefreshCycle(int cycleNumber, CancellationToken cancellationToken)
    {
        var result = await Retry.TryAsync(async () =>
        {
            await UpdateAndDisplayAsync();
            await RenderRefreshTimerAsync(AppConstants.UI.RefreshIntervalSeconds, cancellationToken);
            ClearConsole(); // Side effect isolated
            return Unit.Value; // Unit type for void return
        });

        result.OnFailure(error =>
        {
            _logger.LogError("Error occurred during update cycle: {Error}", error);
            Task.Delay(5000, cancellationToken).Wait(); // Wait before retry
        });
    }

    /// <summary>
    /// I/O side effect: Console clearing isolated to single function.
    /// </summary>
    private static void ClearConsole() => Console.Clear();

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

    /// <summary>
    /// Functional refactored version: Separates pure data aggregation from I/O.
    /// Uses LINQ transformations instead of imperative foreach.
    /// </summary>
    private async Task DisplayEthereumBalancesAsync(CoinGeckoMarketData[] cryptoInfo)
    {
        var balances = await _ethereumBalanceService.GetBalancesAsync(_options.Ethereum.AddressesToMonitor);

        // Pure function: Calculate Ethereum summary data
        var summary = CalculateEthereumSummary(balances, cryptoInfo);

        // I/O side effect: Render to console (isolated)
        RenderEthereumBalances(balances, summary);
    }

    /// <summary>
    /// Pure function: Calculates Ethereum balance summary without side effects.
    /// </summary>
    private static (decimal totalBalance, decimal totalUsdValue, decimal ethPrice) CalculateEthereumSummary(
        Dictionary<string, decimal> balances,
        CoinGeckoMarketData[] cryptoInfo)
    {
        var ethereumPrice = cryptoInfo.FirstOrDefault(r => r.Name == "Ethereum")?.CurrentPrice ?? 0;
        var totalEthBalance = balances.Values.Sum();
        var totalUsdValue = totalEthBalance * ethereumPrice;

        return (totalEthBalance, totalUsdValue, ethereumPrice);
    }

    /// <summary>
    /// I/O side effect: Renders Ethereum balances to console (isolated boundary).
    /// </summary>
    private static void RenderEthereumBalances(
        Dictionary<string, decimal> balances,
        (decimal totalBalance, decimal totalUsdValue, decimal ethPrice) summary)
    {
        Console.WriteLine("\n--- Ethereum Balances ---");

        // Functional iteration: Use LINQ for side effects in isolated I/O boundary
        balances.ToList().ForEach(kvp =>
            Console.WriteLine($"Ethereum balance for {kvp.Key}: {kvp.Value:N8} ETH"));

        Console.WriteLine($"    Total: {summary.totalBalance:N8} ETH (${summary.totalUsdValue:N2})");
    }

    /// <summary>
    /// Functional refactored version: Separates pure data fetching from I/O rendering.
    /// Uses parallel execution and LINQ instead of sequential foreach.
    /// </summary>
    private async Task DisplayBitcoinBalancesAsync(CoinGeckoMarketData[] cryptoInfo)
    {
        var bitcoinPrice = cryptoInfo.FirstOrDefault(r => r.Name == "Bitcoin")?.CurrentPrice ?? 0;

        // Filter valid wallets using pure function
        var validWallets = _options.XpubKeyPairs
            .Where(key => key.Xpub.Length > 4)
            .ToList();

        // Pure function: Fetch wallet data
        var walletDataTasks = validWallets
            .Select(async key => new
            {
                Key = key,
                Balance = await _walletTracker.GetWalletBalanceAsync(key.Xpub, key.ScriptPubKeyType)
            });

        // Parallel execution: Fetch all wallet balances
        var walletData = await Task.WhenAll(walletDataTasks);

        // I/O side effect: Render to console (isolated)
        RenderBitcoinBalances(walletData, bitcoinPrice);
    }

    /// <summary>
    /// I/O side effect: Renders Bitcoin balances to console (isolated boundary).
    /// </summary>
    private static void RenderBitcoinBalances(
        IEnumerable<dynamic> walletData,
        decimal bitcoinPrice)
    {
        Console.WriteLine("\n--- Bitcoin Wallet Values ---");

        // Functional iteration: Use LINQ for side effects in isolated I/O boundary
        walletData.ToList().ForEach(wallet =>
        {
            Console.WriteLine($"\nWallet ({wallet.Key.Xpub[..20]}...) Value:");
            Console.WriteLine($"    In BTC: {wallet.Balance:N8}");
            Console.WriteLine($"    In USD: {wallet.Balance * bitcoinPrice:C2}");
        });
    }

    /// <summary>
    /// Functional refactored version: Countdown timer using async sequence instead of for loop.
    /// No mutable loop counter - uses functional sequence generation.
    /// </summary>
    private static async Task RenderRefreshTimerAsync(int seconds, CancellationToken cancellationToken)
    {
        // Create functional countdown sequence
        var countdown = AsyncSequence.Countdown(seconds, TimeSpan.FromSeconds(1));

        // Process each tick in the sequence
        await foreach (var secondsRemaining in countdown)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            // I/O side effect: Render countdown (isolated)
            RenderCountdownTick(secondsRemaining);
        }
    }

    /// <summary>
    /// I/O side effect: Renders a single countdown tick to console.
    /// </summary>
    private static void RenderCountdownTick(int secondsRemaining)
    {
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write($"Refreshing in {secondsRemaining} seconds... ");
    }
}
