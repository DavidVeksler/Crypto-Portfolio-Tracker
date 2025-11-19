using CryptoTracker.Core.Abstractions;
using CryptoTracker.Core.Configuration;
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
}