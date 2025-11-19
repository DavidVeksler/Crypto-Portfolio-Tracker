namespace CryptoTracker.Core.Abstractions;

/// <summary>
/// Provides Ethereum address balance lookup functionality.
/// </summary>
public interface IEthereumBalanceService
{
    /// <summary>
    /// Gets the balance for a single Ethereum address.
    /// </summary>
    /// <param name="address">The Ethereum address to query.</param>
    Task<decimal> GetBalanceAsync(string address);
}

/// <summary>
/// Provides batch Ethereum address balance lookup functionality.
/// </summary>
public interface IBatchEthereumBalanceService
{
    /// <summary>
    /// Gets balances for multiple Ethereum addresses in a single request.
    /// </summary>
    /// <param name="addresses">The Ethereum addresses to query.</param>
    Task<string[]> GetEthereumBalanceAsync(string[] addresses);
}
