﻿using ElectrumXClient;
using System.Diagnostics;

namespace CryptoTracker.Core.Services.Electrum
{
    public class ElectrumServerProvider
    {
        private static Client? _client = null;

        internal static async Task<Client> GetClientAsync()
        {
            if (_client == null)
            {
                Debug.WriteLine("Establishing a new connection to Electrum server...");
                _client = await ConnectToServerAsync();
            }
            return _client;
        }

        private static async Task<Client> ConnectToServerAsync()
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> server in DefaultElectrumServers.DefaultServers.OrderBy(_ => Guid.NewGuid()).ToList())
            {
                foreach (KeyValuePair<string, string> port in server.Value)
                {
                    try
                    {
                        _client = new Client(server.Key, int.Parse(port.Value), true);
                        ElectrumXClient.Response.ServerVersionResponse? version = await _client.GetServerVersion();
                        if (version is not null)
                        {
                            Debug.WriteLine($"Connected to {server.Key} on port {port.Value}");
                            return _client;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Failed to connect to {server.Key} on port {port.Value}: {ex.Message}");
                        _client = null;
                    }
                }
            }

            throw new Exception("Unable to connect to any Electrum server.");
        }
    }
}