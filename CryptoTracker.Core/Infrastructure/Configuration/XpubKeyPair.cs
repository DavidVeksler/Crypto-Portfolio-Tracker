using NBitcoin;

namespace CryptoTracker.Core.Infrastructure.Configuration;

public class XpubKeyPair
{
    public required string Xpub { get; set; }
    public ScriptPubKeyType ScriptPubKeyType { get; set; }
}