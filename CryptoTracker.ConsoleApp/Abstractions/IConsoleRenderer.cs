using CryptoTracker.Core.Services.CryptoPriceServices;

namespace CryptoTracker.ConsoleApp.CoinStatusRenderingService;

/// <summary>
/// Renders cryptocurrency information to the console.
/// </summary>
public interface IConsoleRenderer
{
    /// <summary>
    /// Renders cryptocurrency price information in a formatted table.
    /// </summary>
    /// <param name="coins">Array of coin information to display.</param>
    void RenderCryptoPrices(CoinInfo[] coins);
}
