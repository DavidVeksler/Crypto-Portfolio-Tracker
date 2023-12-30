using NBitcoin;
using NBitcoin.DataEncoders;
using System.Diagnostics;

public class BitcoinAddressGenerator
{
    private readonly ExtPubKey extKey;

    public static string ZpubToXpub(string zpub)
    {
        // Decode zpub
        byte[] data = Encoders.Base58Check.DecodeData(zpub);

        // Change version bytes to xpub version bytes (0x0488B21E for mainnet)
        data[0] = 0x04;
        data[1] = 0x88;
        data[2] = 0xB2;
        data[3] = 0x1E;

        // Re-encode to xpub
        string xpub = Encoders.Base58Check.EncodeData(data);
        return xpub;
    }

    public BitcoinAddressGenerator(string xpubKey)
    {
        if (xpubKey.StartsWith("zpub"))
        {
            xpubKey = ZpubToXpub(xpubKey);
        }

        extKey = ExtPubKey.Parse(xpubKey, Network.Main);
    }

    public string GetBitcoinAddress(int index, ScriptPubKeyType type)
    {
        if (index < 0)
        {
            throw new ArgumentException("Index must be non-negative.");
        }

        BitcoinAddress newAddress = extKey.Derive(0).Derive((uint)index).PubKey.GetAddress(type, Network.Main);
        Debug.WriteLine(newAddress);
        return newAddress.ToString();
    }

    //    public string GetBitcoinPublicKey(int index, ScriptPubKeyType type)
    //    {
    //        if (index < 0)
    //        {
    //            throw new ArgumentException("Index must be non-negative.");
    //        }        

    //        var pubkey = extKey.Derive((uint)index).GetPublicKey();
    //        return pubkey.ToString();
    //    }
}
