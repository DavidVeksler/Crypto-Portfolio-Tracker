namespace CryptoTracker.Core.Abstractions;

/// <summary>
/// Provides Ethereum address balance lookup functionality.
/// </summary>
public interface IEthereumBalanceService
{
    /// <summary>
    /// Gets the balance in Ether for a single Ethereum address.
    /// </summary>
    /// <param name="address">The Ethereum address to query.</param>
    /// <returns>Balance in Ether (ETH).</returns>
    Task<decimal> GetBalanceAsync(string address);

    /// <summary>
    /// Gets balances in Ether for multiple Ethereum addresses.
    /// </summary>
    /// <param name="addresses">The Ethereum addresses to query.</param>
    /// <returns>Dictionary mapping addresses to their balances in Ether (ETH).</returns>
    Task<Dictionary<string, decimal>> GetBalancesAsync(IEnumerable<string> addresses);
}
