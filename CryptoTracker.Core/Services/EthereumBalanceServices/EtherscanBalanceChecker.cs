using System.Numerics;
using CryptoTracker.Core.Abstractions;
using CryptoTracker.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nethereum.Util;
using Newtonsoft.Json.Linq;

namespace CryptoTracker.Core.Services.EthereumBalanceServices;

/// <summary>
/// Ethereum balance lookup service using Etherscan API.
/// </summary>
public class EtherscanBalanceService : IEthereumBalanceService
{
    private const string BaseUrl = "https://api.etherscan.io/api";
    private readonly HttpClient _httpClient;
    private readonly ILogger<EtherscanBalanceService> _logger;
    private readonly EtherscanOptions _options;

    public EtherscanBalanceService(HttpClient httpClient, IOptions<CryptoTrackerOptions> options,
        ILogger<EtherscanBalanceService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value.Ethereum.Etherscan;
    }

    public async Task<decimal> GetBalanceAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("A valid Ethereum address must be provided.", nameof(address));

        var balances = await GetBalancesAsync(new[] { address });
        return balances.TryGetValue(address, out var balance) ? balance : 0;
    }

    public async Task<Dictionary<string, decimal>> GetBalancesAsync(IEnumerable<string> addresses)
    {
        var addressList = addresses.ToList();
        if (!addressList.Any())
            throw new ArgumentException("No addresses provided.", nameof(addresses));

        var url =
            $"{BaseUrl}?module=account&action=balancemulti&address={string.Join(',', addressList)}&tag=latest&apikey={_options.ApiKey}";

        try
        {
            _logger.LogDebug("Fetching balances for {Count} Ethereum addresses from Etherscan", addressList.Count);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var balances = ParseBalances(responseBody);
            _logger.LogInformation("Successfully fetched {Count} balances from Etherscan", balances.Count);

            return balances;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception when fetching Ethereum balances from Etherscan");
            throw;
        }
    }

    private Dictionary<string, decimal> ParseBalances(string responseBody)
    {
        var json = JObject.Parse(responseBody);
        var balancesArray = json["result"] as JArray;

        if (balancesArray == null || balancesArray.Count == 0)
        {
            _logger.LogWarning("No balances found in Etherscan response");
            return new Dictionary<string, decimal>();
        }

        var balances = new Dictionary<string, decimal>();

        foreach (var balanceItem in balancesArray)
        {
            var account = balanceItem["account"]?.ToString();
            var balanceString = balanceItem["balance"]?.ToString();

            if (string.IsNullOrEmpty(account))
            {
                _logger.LogWarning("Account address is missing in response");
                continue;
            }

            if (BigInteger.TryParse(balanceString, out var balanceWei))
            {
                var etherAmount = UnitConversion.Convert.FromWei(balanceWei);
                balances[account] = etherAmount;
                _logger.LogDebug("Account: {Account}, Balance: {Balance} ETH", account, etherAmount);
            }
            else
            {
                _logger.LogWarning("Invalid or null balance string for account {Account}", account);
                balances[account] = 0;
            }
        }

        return balances;
    }
}