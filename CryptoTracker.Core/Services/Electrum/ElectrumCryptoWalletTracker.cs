using ElectrumXClient;
using NBitcoin;
using System;
using System.Diagnostics;

namespace CryptoTracker.Core.Services.Electrum
{
    public class ElectrumCryptoWalletTracker
    {
        private readonly Client _client;
        private static readonly Dictionary<string, (int LastUsedIndex, DateTime Timestamp)> _addressIndexCache = new();

        public ElectrumCryptoWalletTracker()
        {
            Debug.WriteLine("Initializing Electrum Client...");
            _client = ElectrumServerProvider.GetClientAsync().GetAwaiter().GetResult();
            Debug.WriteLine("Electrum Client initialized.");
        }

        public async Task<int> GetLastUsedAddressIndexAsync(string xpubKey, ScriptPubKeyType keyType)
        {
            Debug.WriteLine($"Getting last used address index for XPubKey: {xpubKey}");
            if (_addressIndexCache.TryGetValue(xpubKey, out (int LastUsedIndex, DateTime Timestamp) cacheEntry))
            {
                Debug.WriteLine($"Found cached entry for XPubKey: {xpubKey}");
                if ((DateTime.UtcNow - cacheEntry.Timestamp).TotalMinutes < 30)
                {
                    Debug.WriteLine($"Cached entry for XPubKey: {xpubKey} is recent. Using cached value.");
                    return cacheEntry.LastUsedIndex;
                }
            }

            Debug.WriteLine($"No valid cache entry found for XPubKey: {xpubKey}. Performing binary search.");
            int lastActiveIndex = await SearchLastUsedIndex(xpubKey, keyType);
            Debug.WriteLine($"Last active index found: {lastActiveIndex}");

            if (lastActiveIndex != -1)
            {
                CacheAddressIndex(xpubKey, lastActiveIndex);
            }
            return lastActiveIndex;
        }

        public async Task<decimal> GetWalletBalanceAsync(string xpub, ScriptPubKeyType scriptPubKeyType)
        {
            Debug.WriteLine($"Getting wallet balance for XPub: {xpub}");
            int lastActiveIndex = await GetLastUsedAddressIndexAsync(xpub, scriptPubKeyType);
            Debug.WriteLine($"Last active index for balance calculation: {lastActiveIndex}");
            return lastActiveIndex == -1 ? 0 : await CalculateTotalBalance(xpub, scriptPubKeyType, lastActiveIndex);
        }

        private async Task<int> SearchLastUsedIndex(string xpubKey, ScriptPubKeyType keyType, int addressGap = 10)
        {
            Debug.WriteLine($"Starting search for last used index for XPubKey: {xpubKey}");
            BitcoinAddressGenerator addressGenerator = new(xpubKey);
            int lastActiveIndex = -1;
            int consecutiveUnusedCount = 0;
            int currentIndex = 0;

            while (consecutiveUnusedCount < addressGap)
            {
                string address = addressGenerator.GenerateAddress(currentIndex, keyType);
                Debug.WriteLine($"Checking address at index {currentIndex}: {address}");
                ElectrumXClient.Response.BlockchainScripthashGetHistoryResponse historyResponse = await _client.GetBlockchainScripthashGetHistory(GetReversedShaHexString(address));
                bool hasTransactions = historyResponse.Result.Count > 0;

                if (hasTransactions)
                {
                    Debug.WriteLine($"Address at index {currentIndex} has transactions. Updating last active index.");
                    lastActiveIndex = currentIndex;
                    consecutiveUnusedCount = 0;
                }
                else
                {
                    Debug.WriteLine($"Address at index {currentIndex} has no transactions. Increasing consecutive unused count.");
                    consecutiveUnusedCount++;
                }

                currentIndex++;
            }

            Debug.WriteLine($"Search completed. Last used index: {lastActiveIndex}");
            return lastActiveIndex;
        }

        private async Task<decimal> CalculateTotalBalance(string xpub, ScriptPubKeyType scriptPubKeyType, int lastActiveIndex)
        {
            long totalBalanceInSatoshis = 0;

            for (int i = 0; i <= lastActiveIndex; i++)
            {
                string address = new BitcoinAddressGenerator(xpub).GenerateAddress(i, scriptPubKeyType);
                long balanceForAddress = await GetBalanceForAddress(address);
                totalBalanceInSatoshis += balanceForAddress;
            }

            var totalBalanceInBTC = SatoshiToBTC(totalBalanceInSatoshis);
            Debug.WriteLine($"Total balance calculated in BTC: {totalBalanceInBTC}");
            return totalBalanceInBTC;
        }



        private async Task<long> GetBalanceForAddress(string address)
        {
            Debug.WriteLine($"Calculating balance for address {address}");

            string reversedSha = GetReversedShaHexString(address);

            ElectrumXClient.Response.BlockchainScripthashGetBalanceResponse balanceResponse = await _client.GetBlockchainScripthashGetBalance(reversedSha);
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
            BitcoinAddress address = BitcoinAddress.Create(publicAddress, Network.Main);
            byte[] sha = NBitcoin.Crypto.Hashes.SHA256(address.ScriptPubKey.ToBytes());
            byte[] reversedSha = sha.Reverse().ToArray();
            string hexString = NBitcoin.DataEncoders.Encoders.Hex.EncodeData(reversedSha);
            // Debug.WriteLine($"Generated reversed SHA hex string: {hexString}");
            return hexString;
        }

        public static decimal SatoshiToBTC(long satoshis)
        {
            Debug.WriteLine($"Converting Satoshi to BTC: {satoshis}");
            try
            {
                Money satoshiMoney = Money.Satoshis(satoshis);
                decimal bitcoins = satoshiMoney.ToUnit(MoneyUnit.BTC);
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
}