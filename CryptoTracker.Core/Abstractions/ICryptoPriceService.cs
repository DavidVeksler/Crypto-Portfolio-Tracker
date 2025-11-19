namespace CryptoTracker.Core.Abstractions;

using Services.CryptoPriceServices;

/// <summary>
/// Provides cryptocurrency price information from external APIs.
/// </summary>
public interface ICryptoPriceService
{
    /// <summary>
    /// Gets the list of supported vs currencies.
    /// </summary>
    Task<string[]> GetSupportedVsCurrenciesAsync();

    /// <summary>
    /// Gets detailed information for specified cryptocurrencies.
    /// </summary>
    /// <param name="vsCurrency">The currency to price against (e.g., "usd").</param>
    /// <param name="ids">Comma-separated list of coin IDs.</param>
    Task<CoinInfo[]> GetCurrencyInfoAsync(string vsCurrency, string ids);
}
