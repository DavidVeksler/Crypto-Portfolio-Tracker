namespace CryptoTracker.Core.Abstractions;

using NBitcoin;

/// <summary>
/// Tracks cryptocurrency wallet balances using blockchain data.
/// </summary>
public interface IWalletTracker
{
    /// <summary>
    /// Gets the last used address index for a given extended public key.
    /// </summary>
    /// <param name="xpubKey">The extended public key.</param>
    /// <param name="keyType">The script type for addresses.</param>
    Task<int> GetLastUsedAddressIndexAsync(string xpubKey, ScriptPubKeyType keyType);

    /// <summary>
    /// Gets the total balance for a wallet identified by its extended public key.
    /// </summary>
    /// <param name="xpub">The extended public key.</param>
    /// <param name="scriptPubKeyType">The script type for addresses.</param>
    Task<decimal> GetWalletBalanceAsync(string xpub, ScriptPubKeyType scriptPubKeyType);
}
