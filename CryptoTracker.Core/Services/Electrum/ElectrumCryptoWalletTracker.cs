using CryptoTracker.Core.Abstractions;
using CryptoTracker.Core.Constants;
using CryptoTracker.Core.Functional;
using CryptoTracker.Core.Services.Bitcoin;
using ElectrumXClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
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

    /// <summary>
    /// Functional refactored version: Searches for the last used address index using immutable state.
    /// Uses async sequence composition with no mutable variables.
    /// </summary>
    private async Task<int> SearchLastUsedIndex(string xpubKey, ScriptPubKeyType keyType, int addressGap = AppConstants.Blockchain.AddressGapLimit)
    {
        _logger.LogDebug("Starting search for last used index for XPubKey: {XPubKey}", xpubKey);

        var addressGenerator = new BitcoinAddressGenerator(xpubKey);
        var client = await _clientProvider.GetClientAsync();

        // Pure function: Creates address check result
        var checkAddress = async (int index) =>
        {
            var address = addressGenerator.GenerateAddress(index, keyType);
            _logger.LogTrace("Checking address at index {Index}: {Address}", index, address);

            var historyResponse = await client.GetBlockchainScripthashGetHistory(GetReversedShaHexString(address));
            var hasTransactions = historyResponse.Result.Count > 0;

            if (hasTransactions)
                _logger.LogDebug("Address at index {Index} has transactions", index);

            return (index, hasTransactions);
        };

        // Immutable state record for tracking consecutive unused addresses
        var initialState = new AddressSearchState(LastActiveIndex: -1, ConsecutiveUnused: 0);

        // Generate infinite sequence of address checks with accumulated state
        var addressCheckSequence = AsyncSequence.UnfoldIndexed(
            initialState,
            async (index, state) =>
            {
                var (_, hasTransactions) = await checkAddress(index);

                // Pure state transformation - no mutations
                var newState = hasTransactions
                    ? new AddressSearchState(LastActiveIndex: index, ConsecutiveUnused: 0)
                    : state with { ConsecutiveUnused = state.ConsecutiveUnused + 1 };

                return (newState, newState);
            });

        // Take elements until we've found the gap limit of consecutive unused addresses
        var finalState = await addressCheckSequence
            .TakeWhile(state => state.ConsecutiveUnused < addressGap)
            .LastOrNone(state => true);

        var lastActiveIndex = finalState
            .Map(state => state.LastActiveIndex)
            .GetOrDefault(-1);

        _logger.LogInformation("Search completed for XPubKey {XPubKey}. Last used index: {Index}", xpubKey, lastActiveIndex);
        return lastActiveIndex;
    }

    // Immutable record for address search state - replaces mutable variables
    private record AddressSearchState(int LastActiveIndex, int ConsecutiveUnused);

    /// <summary>
    /// Functional refactored version: Calculates total balance using immutable fold pattern with parallelization.
    /// No mutable accumulator - uses LINQ Aggregate for functional composition.
    /// </summary>
    private async Task<decimal> CalculateTotalBalance(string xpub, ScriptPubKeyType scriptPubKeyType, int lastActiveIndex)
    {
        var addressGenerator = new BitcoinAddressGenerator(xpub);

        // Pure function: Generate address at index
        var generateAddress = (int index) => addressGenerator.GenerateAddress(index, scriptPubKeyType);

        // Create immutable sequence of indices
        var indices = Enumerable.Range(0, lastActiveIndex + 1);

        // Parallel execution: Map each index to balance fetch task (no sequential awaits)
        var balanceTasks = indices
            .Select(generateAddress)
            .Select(GetBalanceForAddress)
            .ToArray();

        // Execute all balance fetches in parallel
        var balances = await Task.WhenAll(balanceTasks);

        // Pure functional fold: Aggregate balances with no mutable state
        var totalBalanceInSatoshis = balances.Aggregate(0L, (sum, balance) => sum + balance);

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