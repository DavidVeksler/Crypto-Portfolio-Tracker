using ElectrumXClient;
using ElectrumXClient.Response;
using NBitcoin;
using System.Diagnostics;

namespace Console.Bitcoin
{

    public class ElectrumClient
    {
        #region Utilities

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

        #endregion


        public async Task<BlockchainScripthashGetBalanceResponse> GetBalanceAsync(string publicKey)
        {
            Client client = await ElectrumServerProvider.GetClientAsync();
            Debug.WriteLine($"Fetching balance for {publicKey}");
            BlockchainScripthashGetBalanceResponse response = await client.GetBlockchainScripthashGetBalance(GetReversedShaHexString(publicKey));
            Debug.WriteLine($"{publicKey} {response}");
            return response;
        }





        private static readonly Dictionary<string, (int LastUsedIndex, DateTime Timestamp)> _addressIndexCache
        = [];

        // Use a binary search approach to find the last active index
        public async Task<int> GetLastUsedAddressIndex(string xpubKey, ScriptPubKeyType keyType, int transactionGap = 10)
        {
            // Check if a cached value exists and is still valid
            if (_addressIndexCache.TryGetValue(xpubKey, out (int LastUsedIndex, DateTime Timestamp) cacheEntry) &&
                (DateTime.UtcNow - cacheEntry.Timestamp).TotalMinutes < 30)
            {
                return cacheEntry.LastUsedIndex;
            }

            Client client = await ElectrumServerProvider.GetClientAsync();
            BitcoinAddressGenerator generator = new(xpubKey);

            // Check the first address for transactions
            string firstPublicKey = generator.GetBitcoinAddress(0, keyType);
            BlockchainScripthashGetHistoryResponse firstResponse = await client.GetBlockchainScripthashGetHistory(GetReversedShaHexString(firstPublicKey));
            if (firstResponse.Result.Count == 0)
            {
                // No transactions found on the first address, assume wallet is empty
                return -1;
            }

            int lowerBound = 0;
            int upperBound = 100; // Start with a guess
            int lastUsedIndex = -1;

            while (lowerBound <= upperBound)
            {
                int midIndex = (lowerBound + upperBound) / 2;
                string publicKey = generator.GetBitcoinAddress(midIndex, keyType);

                BlockchainScripthashGetHistoryResponse response = await client.GetBlockchainScripthashGetHistory(GetReversedShaHexString(publicKey));

                if (response.Result.Count > 0)
                {
                    lastUsedIndex = midIndex;
                    lowerBound = midIndex + 1;
                }
                else
                {
                    upperBound = midIndex - 1;
                }
            }

            // Cache the result with the current timestamp
            _addressIndexCache[xpubKey] = (lastUsedIndex, DateTime.UtcNow);

            return lastUsedIndex;


        }

        internal async Task<decimal> GetWalletBalanceAsync(string xpub, ScriptPubKeyType scriptPubKeyType)
        {
            Client client = await ElectrumServerProvider.GetClientAsync();
            Debug.WriteLine("Getting last used address index...");
            int lastActiveIndex = await GetLastUsedAddressIndex(xpub, scriptPubKeyType);
            if (lastActiveIndex == -1)
            {
                return 0;
            }

            long runningTotalInSatoshi = 0;
            SemaphoreSlim semaphore = new(1); // Synchronize access

            IEnumerable<Task<int>> tasks = Enumerable.Range(0, lastActiveIndex + 1).Reverse().Select(async i =>
            {
                await semaphore.WaitAsync();
                string address = new BitcoinAddressGenerator(xpub).GetBitcoinAddress(i, scriptPubKeyType);
                try
                {
                    Debug.WriteLine($"Fetching balance for address at index {i}: {address}");
                    BlockchainScripthashGetBalanceResponse response = await GetBalanceAsync(address);

                    int balance = int.Parse(response.Result.Confirmed) + int.Parse(response.Result.Unconfirmed);
                    Debug.WriteLine($"Balance: {balance}");
                    return balance;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"failed to get balance for #{i} ({address}):{ex}");
                    return 0;
                }
                finally
                {
                    _ = semaphore.Release();
                }
            });

            int[] results = await Task.WhenAll(tasks);
            runningTotalInSatoshi = results.Sum();

            return SatoshiToBTC(runningTotalInSatoshi.ToString());
        }



    }


}
