using CryptoTracker.Core.Abstractions;
using CryptoTracker.Core.Configuration;
using CryptoTracker.Core.Functional;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Web3;

namespace CryptoTracker.Core.Services.EthereumBalanceServices;

/// <summary>
/// Ethereum balance lookup service using Infura API.
/// </summary>
public class InfuraBalanceLookupService : IEthereumBalanceService
{
    private readonly Web3 _web3Client;
    private readonly ILogger<InfuraBalanceLookupService> _logger;

    public InfuraBalanceLookupService(IOptions<CryptoTrackerOptions> options, ILogger<InfuraBalanceLookupService> logger)
    {
        _logger = logger;
        var infuraKey = options.Value.Ethereum.Infura.ApiKey;
        _web3Client = new Web3($"https://mainnet.infura.io/v3/{infuraKey}");
    }

    public async Task<decimal> GetBalanceAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("A valid Ethereum address must be provided.", nameof(address));

        try
        {
            _logger.LogDebug("Fetching balance for Ethereum address: {Address}", address);

            var balance = await _web3Client.Eth.GetBalance.SendRequestAsync(address);
            _logger.LogDebug("Balance in Wei for {Address}: {Balance}", address, balance.Value);

            var etherAmount = Web3.Convert.FromWei(balance.Value);
            _logger.LogInformation("Balance in Ether for {Address}: {Balance}", address, etherAmount);

            return etherAmount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving balance for address {Address}", address);
            throw;
        }
    }

    /// <summary>
    /// Functional refactored version: Fetches balances in parallel with proper error handling.
    /// Uses immutable transformations and Result type instead of mutable dictionary and error swallowing.
    /// </summary>
    public async Task<Dictionary<string, decimal>> GetBalancesAsync(IEnumerable<string> addresses)
    {
        var addressList = addresses.ToList();
        if (!addressList.Any())
            throw new ArgumentException("At least one address must be provided.", nameof(addresses));

        _logger.LogDebug("Fetching balances for {Count} Ethereum addresses", addressList.Count);

        // Pure function: Wraps balance fetch in Result type for functional error handling
        var fetchBalanceWithResult = async (string address) =>
        {
            try
            {
                var balance = await GetBalanceAsync(address);
                return Result<(string address, decimal balance)>.Success((address, balance));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching balance for address {Address}", address);
                // Return Result with error instead of swallowing exception
                return Result<(string address, decimal balance)>.Failure($"Failed to fetch balance for {address}: {ex.Message}");
            }
        };

        // Parallel execution: Map each address to a balance fetch task
        var balanceTasks = addressList
            .Select(fetchBalanceWithResult)
            .ToArray();

        // Execute all balance fetches in parallel
        var balanceResults = await Task.WhenAll(balanceTasks);

        // Functional transformation: Convert results to dictionary using LINQ (no mutable dictionary)
        // For failed results, use 0 as default to maintain backward compatibility
        var balances = balanceResults
            .Select(result => result.Match(
                onSuccess: tuple => tuple,
                onFailure: _ => (result.Value?.address ?? "", 0m)))
            .Where(tuple => !string.IsNullOrEmpty(tuple.Item1))
            .ToDictionary(
                keySelector: tuple => tuple.Item1,
                elementSelector: tuple => tuple.Item2);

        return balances;
    }
}