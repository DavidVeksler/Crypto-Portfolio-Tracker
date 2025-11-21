using CryptoTracker.Core.Abstractions;
using CryptoTracker.Core.Constants;
using CryptoTracker.Core.Services.Bitcoin;
using ElectrumXClient;
using Microsoft.Extensions.Caching.Memory;
using NBitcoin;
using NBitcoin.Crypto;
using NBitcoin.DataEncoders;

namespace CryptoTracker.Core.Services.Electrum;

/// <summary>
/// Tracks Bitcoin wallet balances using Electrum servers.
/// </summary>
public class ElectrumCryptoWalletTracker : IWalletTracker
{
    private readonly IMemoryCache _cache;
    private readonly IElectrumClientProvider _clientProvider;
    private readonly ILogger<ElectrumCryptoWalletTracker> _logger;

    public ElectrumCryptoWalletTracker(
        IElectrumClientProvider clientProvider,
        ILogger<ElectrumCryptoWalletTracker> logger,
        IMemoryCache cache)
    {
        _clientProvider = clientProvider;
        _logger = logger;
        _cache = cache;
    }

    public async Task<int> GetLastUsedAddressIndexAsync(string xpubKey, ScriptPubKeyType keyType)
    {
        _logger.LogDebug("Getting last used address index for XPubKey: {XPubKey}", xpubKey);

        var cacheKey = $"AddressIndex_{xpubKey}";
        if (_cache.TryGetValue(cacheKey, out int cachedIndex))
        {
            _logger.LogDebug("Using cached value for XPubKey: {XPubKey}", xpubKey);
            return cachedIndex;
        }

        _logger.LogDebug("No valid cache found for XPubKey: {XPubKey}. Performing search.", xpubKey);
        var lastActiveIndex = await SearchLastUsedIndex(xpubKey, keyType);
        _logger.LogInformation("Last active index for XPubKey {XPubKey}: {Index}", xpubKey, lastActiveIndex);

        if (lastActiveIndex != -1)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(AppConstants.Blockchain.AddressIndexCacheTimeoutMinutes));
            _cache.Set(cacheKey, lastActiveIndex, cacheOptions);
            _logger.LogDebug("Cached last used index {Index} for XPubKey: {XPubKey}", lastActiveIndex, xpubKey);
        }

        return lastActiveIndex;
    }

    public async Task<decimal> GetWalletBalanceAsync(string xpub, ScriptPubKeyType scriptPubKeyType)
    {
        _logger.LogDebug("Getting wallet balance for XPub: {XPub}", xpub);
        var lastActiveIndex = await GetLastUsedAddressIndexAsync(xpub, scriptPubKeyType);
        _logger.LogDebug("Last active index for balance calculation: {Index}", lastActiveIndex);
        return lastActiveIndex == -1 ? 0 : await CalculateTotalBalance(xpub, scriptPubKeyType, lastActiveIndex);
    }

    private async Task<int> SearchLastUsedIndex(string xpubKey, ScriptPubKeyType keyType, int addressGap = AppConstants.Blockchain.AddressGapLimit)
    {
        _logger.LogDebug("Starting search for last used index for XPubKey: {XPubKey}", xpubKey);

        var addressGenerator = new BitcoinAddressGenerator(xpubKey);
        var lastActiveIndex = -1;
        var consecutiveUnusedCount = 0;
        var currentIndex = 0;
        var client = await _clientProvider.GetClientAsync();

        while (consecutiveUnusedCount < addressGap)
        {
            var address = addressGenerator.GenerateAddress(currentIndex, keyType);
            _logger.LogTrace("Checking address at index {Index}: {Address}", currentIndex, address);

            var historyResponse = await client.GetBlockchainScripthashGetHistory(GetReversedShaHexString(address));
            var hasTransactions = historyResponse.Result.Count > 0;

            if (hasTransactions)
            {
                _logger.LogDebug("Address at index {Index} has transactions", currentIndex);
                lastActiveIndex = currentIndex;
                consecutiveUnusedCount = 0;
            }
            else
            {
                consecutiveUnusedCount++;
            }

            currentIndex++;
        }

        _logger.LogInformation("Search completed for XPubKey {XPubKey}. Last used index: {Index}", xpubKey,
            lastActiveIndex);
        return lastActiveIndex;
    }

    private async Task<decimal> CalculateTotalBalance(string xpub, ScriptPubKeyType scriptPubKeyType,
        int lastActiveIndex)
    {
        long totalBalanceInSatoshis = 0;
        var addressGenerator = new BitcoinAddressGenerator(xpub);

        for (var i = 0; i <= lastActiveIndex; i++)
        {
            var address = addressGenerator.GenerateAddress(i, scriptPubKeyType);
            var balanceForAddress = await GetBalanceForAddress(address);
            totalBalanceInSatoshis += balanceForAddress;
        }

        var totalBalanceInBTC = SatoshiToBTC(totalBalanceInSatoshis);
        _logger.LogInformation("Total balance calculated in BTC: {Balance}", totalBalanceInBTC);
        return totalBalanceInBTC;
    }


    private async Task<long> GetBalanceForAddress(string address)
    {
        _logger.LogTrace("Calculating balance for address {Address}", address);

        var reversedSha = GetReversedShaHexString(address);
        var client = await _clientProvider.GetClientAsync();
        var balanceResponse = await client.GetBlockchainScripthashGetBalance(reversedSha);

        return long.Parse(balanceResponse.Result.Confirmed) + long.Parse(balanceResponse.Result.Unconfirmed);
    }

    private static string GetReversedShaHexString(string publicAddress)
    {
        var address = BitcoinAddress.Create(publicAddress, Network.Main);
        var sha = Hashes.SHA256(address.ScriptPubKey.ToBytes());
        var reversedSha = sha.Reverse().ToArray();
        return Encoders.Hex.EncodeData(reversedSha);
    }

    private static decimal SatoshiToBTC(long satoshis)
    {
        try
        {
            var satoshiMoney = Money.Satoshis(satoshis);
            return satoshiMoney.ToUnit(MoneyUnit.BTC);
        }
        catch
        {
            return 0;
        }
    }
}