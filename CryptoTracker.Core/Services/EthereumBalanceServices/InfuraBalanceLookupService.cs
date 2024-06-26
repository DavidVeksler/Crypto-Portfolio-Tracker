﻿using System.Diagnostics;
using CryptoTracker.Core.Infrastructure.Configuration;
using Nethereum.Web3;

namespace Console.Ethereum;

public class InfuraBalanceLookupService
{
    private readonly Web3 _web3Client;

    public InfuraBalanceLookupService()
    {
        _web3Client = new Web3("https://mainnet.infura.io/v3/" + ConfigSettings.InfuraKey);
    }

    public async Task<decimal> GetBalanceAsync(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("A valid Ethereum address must be provided.", nameof(address));

        try
        {
            var balance = await _web3Client.Eth.GetBalance.SendRequestAsync(address);
            Debug.WriteLine($"Balance in Wei: {balance.Value}");

            var etherAmount = Web3.Convert.FromWei(balance.Value);
            Debug.WriteLine($"Balance in Ether: {etherAmount}");
            return etherAmount;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error occurred while retrieving balance: {ex.Message}");
            throw;
        }
    }
}