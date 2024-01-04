using CryptoTracker.Core.Infrastructure.Configuration;
using Nethereum.Util;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Numerics;

public static class EtherscanBalanceChecker
{
    private static readonly HttpClient Client = new();    
    private const string BaseUrl = "https://api.etherscan.io/api";

    public static async Task<string[]> GetEthereumBalanceAsync(string[] addresses)
    {
        if (addresses == null || addresses.Length == 0)
        {
            throw new ArgumentException("No addresses provided.", nameof(addresses));
        }

        string url = $"{BaseUrl}?module=account&action=balancemulti&address={string.Join(',', addresses)}&tag=latest&apikey={ConfigSettings.EtherscanKey}";

        try
        {
            HttpResponseMessage response = await Client.GetAsync(url);
            _ = response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

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
        JObject json = JObject.Parse(responseBody);
        JArray? balancesArray = json["result"] as JArray;

        if (balancesArray == null || balancesArray.Count == 0)
        {
            Debug.WriteLine("No balances found in the response.");
            return new string[] { "No balances" };
        }

        List<string> balanceStrings = new List<string>();

        foreach (var balanceItem in balancesArray)
        {
            string account = balanceItem["account"]?.ToString() ?? "Unknown Account";
            string balanceString = balanceItem["balance"]?.ToString();

            if (BigInteger.TryParse(balanceString, out BigInteger balance))
            {
                decimal etherAmount = UnitConversion.Convert.FromWei(balance);
                balanceStrings.Add($"Account: {account}, Balance in Ether: {etherAmount:N}");
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
