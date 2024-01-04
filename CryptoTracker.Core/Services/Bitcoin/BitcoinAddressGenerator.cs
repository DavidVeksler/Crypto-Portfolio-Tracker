using NBitcoin;
using NBitcoin.DataEncoders;

public class BitcoinAddressGenerator
{
    private readonly ExtPubKey _extKey;

    public BitcoinAddressGenerator(string xpubKey)
    {
        _extKey = ExtPubKey.Parse(ConvertIfZpub(xpubKey), Network.Main);
    }

    public string GenerateAddress(int index, ScriptPubKeyType type)
    {
        ValidateIndex(index);
        BitcoinAddress address = _extKey.Derive(0).Derive((uint)index).PubKey.GetAddress(type, Network.Main);
        return address.ToString();
    }

    private static string ConvertIfZpub(string key)
    {
        return key.StartsWith("zpub") ? ZpubToXpub(key) : key;
    }

    private static void ValidateIndex(int index)
    {
        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Index must be non-negative.");
        }
    }

    private static string ZpubToXpub(string zpub)
    {
        byte[] data = Encoders.Base58Check.DecodeData(zpub);
        data[0] = 0x04; data[1] = 0x88; data[2] = 0xB2; data[3] = 0x1E;
        return Encoders.Base58Check.EncodeData(data);
    }
}