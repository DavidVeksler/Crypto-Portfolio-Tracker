namespace CryptoTracker.Core.Abstractions;

using ElectrumXClient;

/// <summary>
/// Provides Electrum server client connections.
/// </summary>
public interface IElectrumClientProvider
{
    /// <summary>
    /// Gets an Electrum client connection, creating one if necessary.
    /// </summary>
    Task<Client> GetClientAsync();
}
