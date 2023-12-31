using ElectrumXClient;
using NBitcoin;
using System.Diagnostics;

namespace Console.Bitcoin
{
    public class ElectrumClient
    {
        private Client _client;

        public ElectrumClient()
        {
            _client = ElectrumServerProvider.GetClientAsync().GetAwaiter().GetResult();
        }


        public async Task<int> GetLastUsedAddressIndexAsync(string xpubKey, ScriptPubKeyType keyType)
        {
            // Check cache first
            if (_addressIndexCache.TryGetValue(xpubKey, out var cacheEntry))
            {
                // Check if the cached value is still recent, e.g., less than 30 minutes old
                if ((DateTime.UtcNow - cacheEntry.Timestamp).TotalMinutes < 30)
                {
                    return cacheEntry.LastUsedIndex;
                }
            }

            // Perform binary search if no valid cache entry is found
            int lastActiveIndex = await SearchLastUsedIndex(xpubKey, keyType);
            if (lastActiveIndex != -1)
            {
                CacheAddressIndex(xpubKey, lastActiveIndex);
            }
            return lastActiveIndex;
        }


        public async Task<decimal> GetWalletBalanceAsync(string xpub, ScriptPubKeyType scriptPubKeyType)
        {
            int lastActiveIndex = await GetLastUsedAddressIndexAsync(xpub, scriptPubKeyType);
            return lastActiveIndex == -1 ? 0 : await CalculateTotalBalance(xpub, scriptPubKeyType, lastActiveIndex);
        }

        private async Task<int> SearchLastUsedIndex(string xpubKey, ScriptPubKeyType keyType, int addressGap = 10)
        {
            BitcoinAddressGenerator addressGenerator = new BitcoinAddressGenerator(xpubKey);
            int lastActiveIndex = -1;
            int consecutiveUnusedCount = 0;
            int currentIndex = 0;

            while (consecutiveUnusedCount < addressGap)
            {
                string address = addressGenerator.GenerateAddress(currentIndex, keyType);
                var historyResponse = await _client.GetBlockchainScripthashGetHistory(GetReversedShaHexString(address));
                bool hasTransactions = historyResponse.Result.Count > 0;

                if (hasTransactions)
                {
                    lastActiveIndex = currentIndex;
                    consecutiveUnusedCount = 0; // Reset the count as a used address is found
                }
                else
                {
                    consecutiveUnusedCount++;
                }

                currentIndex++;
            }

            return lastActiveIndex; // This will be the index of the last used address
        }


        private async Task<decimal> CalculateTotalBalance(string xpub, ScriptPubKeyType scriptPubKeyType, int lastActiveIndex)
        {
            long totalBalanceInSatoshis = 0;
            BitcoinAddressGenerator addressGenerator = new BitcoinAddressGenerator(xpub);

            for (int i = 0; i <= lastActiveIndex; i++)
            {
                string address = addressGenerator.GenerateAddress(i, scriptPubKeyType);
                string reversedSha = GetReversedShaHexString(address);

                var balanceResponse = await _client.GetBlockchainScripthashGetBalance(reversedSha);
                totalBalanceInSatoshis += long.Parse(balanceResponse.Result.Confirmed) + long.Parse(balanceResponse.Result.Unconfirmed);
            }

            return SatoshiToBTC(totalBalanceInSatoshis.ToString());
        }


        private static readonly Dictionary<string, (int LastUsedIndex, DateTime Timestamp)> _addressIndexCache = new Dictionary<string, (int, DateTime)>();

        private void CacheAddressIndex(string xpubKey, int index)
        {
            // Cache the index along with the current timestamp
            _addressIndexCache[xpubKey] = (index, DateTime.UtcNow);
        }

        public static string GetReversedShaHexString(string publicAddress)
        {
            BitcoinAddress address = BitcoinAddress.Create(publicAddress, Network.Main);
            byte[] sha = NBitcoin.Crypto.Hashes.SHA256(address.ScriptPubKey.ToBytes());
            byte[] reversedSha = sha.Reverse().ToArray();
            return NBitcoin.DataEncoders.Encoders.Hex.EncodeData(reversedSha);
        }

        public static decimal SatoshiToBTC(string satoshiString)
        {
            try
            {
                long satoshis = long.Parse(satoshiString);
                Money satoshiMoney = Money.Satoshis(satoshis);
                decimal bitcoins = satoshiMoney.ToUnit(MoneyUnit.BTC);
                return bitcoins;
            }
            catch (Exception ex)
            {
                // Handle potential errors, like invalid input format
                Debug.WriteLine($"Error: {ex.Message}");
                return 0;
            }
        }

    }


}
