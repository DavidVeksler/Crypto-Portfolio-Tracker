using System.Diagnostics;
using System.Numerics;
using CryptoTracker.Core.Infrastructure.Configuration;
using Nethereum.Util;
using Newtonsoft.Json.Linq;

public static class EtherscanBalanceChecker
{
    private const string BaseUrl = "https://api.etherscan.io/api";
    private static readonly HttpClient Client = new();

    public static async Task<string[]> GetEthereumBalanceAsync(string[] addresses)
    {
        if (addresses == null || addresses.Length == 0)
            throw new ArgumentException("No addresses provided.", nameof(addresses));

        var url =
            $"{BaseUrl}?module=account&action=balancemulti&address={string.Join(',', addresses)}&tag=latest&apikey={ConfigSettings.EtherscanKey}";

        try
        {
            var response = await Client.GetAsync(url);
            _ = response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            return ParseBalances(responseBody);
        }
        catch (HttpRequestException e)
        {
            Debug.WriteLine($"HttpRequestException Caught: {e.Message}");
            throw;
        }
    }

    private static string[] ParseBalances(string responseBody)
    {
        var json = JObject.Parse(responseBody);
        var balancesArray = json["result"] as JArray;

        if (balancesArray == null || balancesArray.Count == 0)
        {
            Debug.WriteLine("No balances found in the response.");
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
                Debug.WriteLine($"Account: {account}, Balance in Ether: {etherAmount:N}");
            }
            else
            {
                Debug.WriteLine($"Invalid or null balance string for account {account}");
                balanceStrings.Add($"Account: {account}, Invalid balance");
            }
        }

        return balanceStrings.ToArray();
    }
}