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
public class EtherscanBalanceService : IBatchEthereumBalanceService
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

    public async Task<string[]> GetEthereumBalanceAsync(string[] addresses)
    {
        if (addresses == null || addresses.Length == 0)
            throw new ArgumentException("No addresses provided.", nameof(addresses));

        var url =
            $"{BaseUrl}?module=account&action=balancemulti&address={string.Join(',', addresses)}&tag=latest&apikey={_options.ApiKey}";

        try
        {
            _logger.LogDebug("Fetching balances for {Count} Ethereum addresses from Etherscan", addresses.Length);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            var balances = ParseBalances(responseBody);
            _logger.LogInformation("Successfully fetched {Count} balances from Etherscan", balances.Length);

            return balances;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP request exception when fetching Ethereum balances from Etherscan");
            throw;
        }
    }

    private string[] ParseBalances(string responseBody)
    {
        var json = JObject.Parse(responseBody);
        var balancesArray = json["result"] as JArray;

        if (balancesArray == null || balancesArray.Count == 0)
        {
            _logger.LogWarning("No balances found in Etherscan response");
            return new[] { "No balances" };
        }

        var balanceStrings = new List<string>();

        foreach (var balanceItem in balancesArray)
        {
            var account = balanceItem["account"]?.ToString() ?? "Unknown Account";
            var balanceString = balanceItem["balance"]?.ToString();

            if (BigInteger.TryParse(balanceString, out var balance))
            {
                var etherAmount = UnitConversion.Convert.FromWei(balance);
                balanceStrings.Add(etherAmount.ToString());
                _logger.LogDebug("Account: {Account}, Balance: {Balance} ETH", account, etherAmount);
            }
            else
            {
                _logger.LogWarning("Invalid or null balance string for account {Account}", account);
                balanceStrings.Add($"Account: {account}, Invalid balance");
            }
        }

        return balanceStrings.ToArray();
    }
}