using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;
using NBitcoin;

public class BitcoinAddressGenerator
{
    private ExtPubKey extKey;

    public BitcoinAddressGenerator(string xpubKey)
    {        
        extKey = ExtPubKey.Parse(xpubKey,Network.Main);
    }

    public string GetBitcoinAddress(int index, ScriptPubKeyType type)
    {
        if (index < 0)
        {
            throw new ArgumentException("Index must be non-negative.");
        }

        BitcoinAddress address = extKey.Derive((uint)index).GetPublicKey().GetAddress(type, Network.Main);
        return address.ToString();
    }
}
