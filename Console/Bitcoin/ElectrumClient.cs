using ElectrumXClient;
using ElectrumXClient.Request;
using ElectrumXClient.Response;
using System.Diagnostics;
using System.Net;

namespace Console.Bitcoin
{

    public class ElectrumClient
    {

        public async Task<decimal> GetBalanceAsync(string address)
        {
            const string host = "157.245.172.236";
            Debug.WriteLine($"fetch balance for {address}");
            var client = new Client(host, 50002, true);
            var balance = client.GetBlockchainScripthashGetBalance(address).Result;

            Debug.WriteLine($"Confirmed transactions: {balance.Result.Confirmed != null}");
            return balance.Result.Confirmed.Sum(s => s);
        }
    }


}
