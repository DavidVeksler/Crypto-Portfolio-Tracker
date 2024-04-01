namespace CryptoTracker.Core.Services.Electrum;

internal class DefaultElectrumServers
{
    private static readonly Dictionary<string, string> DefaultPorts = new()
    {
        { "s", "50002" },
        { "t", "50001" }
    };

    // https://github.com/spesmilo/electrum/blob/afa1a4d22a31d23d088c6670e1588eed32f7114d/lib/network.py#L57
    public static readonly Dictionary<string, Dictionary<string, string>> DefaultServers = new()
    {
        { "electrum.bitaroo.net", DefaultPorts }
        //{"erbium1.sytes.net", DefaultPorts},
        //{"ecdsa.net", new Dictionary<string, string> {{"t", "50001"}, {"s", "110"}}},
        //{"gh05.geekhosters.com", DefaultPorts},
        //{"VPS.hsmiths.com", DefaultPorts},
        //{"electrum.anduck.net", DefaultPorts},
        //{"electrum.no-ip.org", DefaultPorts},
        //{"electrum.be", DefaultPorts},
        //{"helicarrier.bauerj.eu", DefaultPorts},
        //{"elex01.blackpole.online", DefaultPorts},
        //{"electrumx.not.fyi", DefaultPorts},
        //{"node.xbt.eu", DefaultPorts},
        //{"kirsche.emzy.de", DefaultPorts},
        //{"electrum.villocq.com", DefaultPorts},
        //{"us11.einfachmalnettsein.de", DefaultPorts},
        //{"electrum.trouth.net", DefaultPorts},
        //{"electrum3.hachre.de", DefaultPorts},
        //{"b.1209k.com", DefaultPorts},
        //{"elec.luggs.co", new Dictionary<string, string> {{"s", "443"}}},
        //{"btc.smsys.me", new Dictionary<string, string> {{"t", "110"}, {"s", "995"}}}
    };
}