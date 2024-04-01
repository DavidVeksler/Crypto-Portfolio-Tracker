using System.Diagnostics;
using ElectrumXClient;
using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;

namespace CryptoTracker.Core.Services.Electrum;

public class ElectrumCryptoWalletTracker
{
    private static readonly Dictionary<string, (int LastUsedIndex, DateTime Timestamp)> _addressIndexCache = new();
    private readonly Client _client;

    public ElectrumCryptoWalletTracker()
    {
        Debug.WriteLine("Initializing Electrum Client...");
        _client = ElectrumServerProvider.GetClientAsync().GetAwaiter().GetResult();
        Debug.WriteLine("Electrum Client initialized.");
    }

    public async Task<int> GetLastUsedAddressIndexAsync(string xpubKey, ScriptPubKeyType keyType)
    {
        Debug.WriteLine($"Getting last used address index for XPubKey: {xpubKey}");
        if (_addressIndexCache.TryGetValue(xpubKey, out var cacheEntry))
        {
            Debug.WriteLine($"Found cached entry for XPubKey: {xpubKey}");
            if ((DateTime.UtcNow - cacheEntry.Timestamp).TotalMinutes < 30)
            {
                Debug.WriteLine($"Cached entry for XPubKey: {xpubKey} is recent. Using cached value.");
                return cacheEntry.LastUsedIndex;
            }
        }

        Debug.WriteLine($"No valid cache entry found for XPubKey: {xpubKey}. Performing binary search.");
        var lastActiveIndex = await SearchLastUsedIndex(xpubKey, keyType);
        Debug.WriteLine($"Last active index found: {lastActiveIndex}");

        if (lastActiveIndex != -1) CacheAddressIndex(xpubKey, lastActiveIndex);
        return lastActiveIndex;
    }

    public async Task<decimal> GetWalletBalanceAsync(string xpub, ScriptPubKeyType scriptPubKeyType)
    {
        Debug.WriteLine($"Getting wallet balance for XPub: {xpub}");
        var lastActiveIndex = await GetLastUsedAddressIndexAsync(xpub, scriptPubKeyType);
        Debug.WriteLine($"Last active index for balance calculation: {lastActiveIndex}");
        return lastActiveIndex == -1 ? 0 : await CalculateTotalBalance(xpub, scriptPubKeyType, lastActiveIndex);
    }

    private async Task<int> SearchLastUsedIndex(string xpubKey, ScriptPubKeyType keyType, int addressGap = 10)
    {
        Debug.WriteLine($"Starting search for last used index for XPubKey: {xpubKey}");
        BitcoinAddressGenerator addressGenerator = new(xpubKey);
        var lastActiveIndex = -1;
        var consecutiveUnusedCount = 0;
        var currentIndex = 0;

        while (consecutiveUnusedCount < addressGap)
        {
            var address = addressGenerator.GenerateAddress(currentIndex, keyType);
            Debug.WriteLine($"Checking address at index {currentIndex}: {address}");
            var historyResponse = await _client.GetBlockchainScripthashGetHistory(GetReversedShaHexString(address));
            var hasTransactions = historyResponse.Result.Count > 0;

            if (hasTransactions)
            {
                Debug.WriteLine($"Address at index {currentIndex} has transactions. Updating last active index.");
                lastActiveIndex = currentIndex;
                consecutiveUnusedCount = 0;
            }
            else
            {
                Debug.WriteLine(
                    $"Address at index {currentIndex} has no transactions. Increasing consecutive unused count.");
                consecutiveUnusedCount++;
            }

            currentIndex++;
        }

        Debug.WriteLine($"Search completed. Last used index: {lastActiveIndex}");
        return lastActiveIndex;
    }

    private async Task<decimal> CalculateTotalBalance(string xpub, ScriptPubKeyType scriptPubKeyType,
        int lastActiveIndex)
    {
        long totalBalanceInSatoshis = 0;

        for (var i = 0; i <= lastActiveIndex; i++)
        {
            var address = new BitcoinAddressGenerator(xpub).GenerateAddress(i, scriptPubKeyType);
            var balanceForAddress = await GetBalanceForAddress(address);
            totalBalanceInSatoshis += balanceForAddress;
        }

        var totalBalanceInBTC = SatoshiToBTC(totalBalanceInSatoshis);
        Debug.WriteLine($"Total balance calculated in BTC: {totalBalanceInBTC}");
        return totalBalanceInBTC;
    }


    private async Task<long> GetBalanceForAddress(string address)
    {
        Debug.WriteLine($"Calculating balance for address {address}");

        var reversedSha = GetReversedShaHexString(address);

        var balanceResponse = await _client.GetBlockchainScripthashGetBalance(reversedSha);
        return long.Parse(balanceResponse.Result.Confirmed) + long.Parse(balanceResponse.Result.Unconfirmed);
    }

    private void CacheAddressIndex(string xpubKey, int index)
    {
        Debug.WriteLine($"Caching last used index for XPubKey: {xpubKey}");
        _addressIndexCache[xpubKey] = (index, DateTime.UtcNow);
        Debug.WriteLine($"Cache updated for XPubKey: {xpubKey}");
    }

    public static string GetReversedShaHexString(string publicAddress)
    {
        // Debug.WriteLine($"Generating reversed SHA hex string for public address: {publicAddress}");
        var address = BitcoinAddress.Create(publicAddress, Network.Main);
        var sha = Hashes.SHA256(address.ScriptPubKey.ToBytes());
        var reversedSha = sha.Reverse().ToArray();
        var hexString = Encoders.Hex.EncodeData(reversedSha);
        // Debug.WriteLine($"Generated reversed SHA hex string: {hexString}");
        return hexString;
    }

    public static decimal SatoshiToBTC(long satoshis)
    {
        Debug.WriteLine($"Converting Satoshi to BTC: {satoshis}");
        try
        {
            var satoshiMoney = Money.Satoshis(satoshis);
            var bitcoins = satoshiMoney.ToUnit(MoneyUnit.BTC);
            Debug.WriteLine($"Conversion result: {bitcoins} BTC");
            return bitcoins;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in converting Satoshi to BTC: {ex.Message}");
            return 0;
        }
    }
}