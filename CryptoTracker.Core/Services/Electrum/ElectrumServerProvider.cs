using CryptoTracker.Core.Abstractions;
using CryptoTracker.Core.Functional;
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

    /// <summary>
    /// Functional refactored version: Uses pure retry logic with Result type instead of nested loops.
    /// No early returns or exception-based control flow in the main logic.
    /// </summary>
    private async Task<Client> ConnectToServerAsync()
    {
        var servers = DefaultElectrumServers.DefaultServers.OrderBy(_ => Guid.NewGuid()).ToList();
        _logger.LogDebug("Attempting to connect to {Count} Electrum servers", servers.Count);

        // Pure function: Creates server endpoint from server and port
        var serverEndpoints = servers
            .SelectMany(server => server.Value.Select(port => (server: server.Key, port: port.Value)))
            .ToList();

        // Pure function: Attempts connection to a single endpoint
        var tryConnectToEndpoint = async ((string server, string port) endpoint) =>
        {
            _logger.LogDebug("Trying to connect to {Server} on port {Port}", endpoint.server, endpoint.port);

            return await Retry.TryAsync(async () =>
            {
                var client = new Client(endpoint.server, int.Parse(endpoint.port), true);
                var version = await client.GetServerVersion();

                if (version is not null)
                {
                    _logger.LogInformation("Successfully connected to {Server} on port {Port}",
                        endpoint.server, endpoint.port);
                    return client;
                }

                throw new InvalidOperationException($"Server {endpoint.server}:{endpoint.port} returned null version");
            });
        };

        // Functional composition: Create sequence of retry operations
        var connectionAttempts = Retry.CreateRetrySequence(serverEndpoints, tryConnectToEndpoint);

        // Execute retry sequence until first success
        var result = await Retry.FirstSuccessWithLog(
            connectionAttempts,
            onAttempt: msg => _logger.LogDebug(msg),
            onFailure: msg => _logger.LogWarning(msg));

        // Pattern matching on Result: Extract client or throw exception
        return result.Match(
            onSuccess: client =>
            {
                _client = client; // Update mutable state only on success
                return client;
            },
            onFailure: error =>
            {
                _logger.LogError("Unable to connect to any Electrum server: {Error}", error);
                throw new InvalidOperationException($"Unable to connect to any Electrum server: {error}");
            });
    }
}