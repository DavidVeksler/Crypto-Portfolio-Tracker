using CryptoTracker.Core.Abstractions;
using ElectrumXClient;
using Microsoft.Extensions.Logging;

namespace CryptoTracker.Core.Services.Electrum;

/// <summary>
/// Provides Electrum server client connections with automatic failover.
/// </summary>
public class ElectrumServerProvider : IElectrumClientProvider
{
    private readonly ILogger<ElectrumServerProvider> _logger;
    private Client? _client;

    public ElectrumServerProvider(ILogger<ElectrumServerProvider> logger)
    {
        _logger = logger;
    }

    public async Task<Client> GetClientAsync()
    {
        if (_client == null)
        {
            _logger.LogInformation("Establishing a new connection to Electrum server...");
            _client = await ConnectToServerAsync();
        }

        return _client;
    }

    private async Task<Client> ConnectToServerAsync()
    {
        var servers = DefaultElectrumServers.DefaultServers.OrderBy(_ => Guid.NewGuid()).ToList();
        _logger.LogDebug("Attempting to connect to {Count} Electrum servers", servers.Count);

        foreach (var server in servers)
        {
            foreach (var port in server.Value)
            {
                try
                {
                    _logger.LogDebug("Trying to connect to {Server} on port {Port}", server.Key, port.Value);

                    _client = new Client(server.Key, int.Parse(port.Value), true);
                    var version = await _client.GetServerVersion();

                    if (version is not null)
                    {
                        _logger.LogInformation("Successfully connected to {Server} on port {Port}", server.Key,
                            port.Value);
                        return _client;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to connect to {Server} on port {Port}", server.Key, port.Value);
                    _client = null;
                }
            }
        }

        _logger.LogError("Unable to connect to any Electrum server");
        throw new InvalidOperationException("Unable to connect to any Electrum server.");
    }
}