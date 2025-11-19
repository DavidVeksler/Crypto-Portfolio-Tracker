namespace CryptoTracker.Core.Abstractions;

using NBitcoin;

/// <summary>
/// Generates cryptocurrency addresses from extended public keys.
/// </summary>
public interface IAddressGenerator
{
    /// <summary>
    /// Generates an address at the specified index using the given script type.
    /// </summary>
    /// <param name="index">The derivation index.</param>
    /// <param name="type">The script type for the address.</param>
    string GenerateAddress(int index, ScriptPubKeyType type);
}
