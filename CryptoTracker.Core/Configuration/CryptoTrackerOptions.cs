namespace CryptoTracker.Core.Configuration;

using Infrastructure.Configuration;

/// <summary>
/// Configuration options for the crypto tracker application.
/// </summary>
public class CryptoTrackerOptions
{
    /// <summary>
    /// Gets or sets the comma-separated list of cryptocurrency IDs to track.
    /// </summary>
    public string PricesToCheck { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CoinGecko API configuration.
    /// </summary>
    public CoinGeckoOptions CoinGecko { get; set; } = new();

    /// <summary>
    /// Gets or sets the Ethereum configuration.
    /// </summary>
    public EthereumOptions Ethereum { get; set; } = new();

    /// <summary>
    /// Gets or sets the Bitcoin wallet configuration.
    /// </summary>
    public List<XpubKeyPair> XpubKeyPairs { get; set; } = new();
}

/// <summary>
/// CoinGecko API configuration options.
/// </summary>
public class CoinGeckoOptions
{
    /// <summary>
    /// Gets or sets the CoinGecko API key.
    /// </summary>
    public string? ApiKey { get; set; }
}

/// <summary>
/// Ethereum network configuration options.
/// </summary>
public class EthereumOptions
{
    /// <summary>
    /// Gets or sets the list of Ethereum addresses to monitor.
    /// </summary>
    public List<string> AddressesToMonitor { get; set; } = new();

    /// <summary>
    /// Gets or sets the comma-separated list of tokens to track.
    /// </summary>
    public string? TokensToTrack { get; set; }

    /// <summary>
    /// Gets or sets the Infura API configuration.
    /// </summary>
    public InfuraOptions Infura { get; set; } = new();

    /// <summary>
    /// Gets or sets the Etherscan API configuration.
    /// </summary>
    public EtherscanOptions Etherscan { get; set; } = new();
}

/// <summary>
/// Infura API configuration options.
/// </summary>
public class InfuraOptions
{
    /// <summary>
    /// Gets or sets the Infura API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}

/// <summary>
/// Etherscan API configuration options.
/// </summary>
public class EtherscanOptions
{
    /// <summary>
    /// Gets or sets the Etherscan API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
