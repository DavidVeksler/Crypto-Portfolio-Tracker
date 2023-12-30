using Microsoft.Extensions.Configuration;
using NBitcoin;


public class XpubKeyPair
{
    public required string Xpub { get; set; }
    public ScriptPubKeyType ScriptPubKeyType { get; set; }
}

public class Settings
{
    internal static IConfigurationRoot? configuration;

    private Settings()
    {

    }

    private static void InitConfiguration()
    {
        string path = Directory.GetCurrentDirectory();
        if (!File.Exists(path + "\\appsettings.json"))
        {
            path = @"C:\\Users\\veksl\\Projects\\Crypto-Portfolio-Tracker\\Console";
        }
        if (!File.Exists(path + "\\appsettings.json")) { throw new Exception("appsettings.json is required"); }


        IConfigurationBuilder builder = new ConfigurationBuilder()
        .SetBasePath(path)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        configuration = builder.Build();

    }

    public static string OpenAIKey
    {
        get
        {
            if (configuration == null)
            {
                InitConfiguration();
            }

            string apiKey = configuration["OpenAI:ApiKey"];
            return apiKey;
        }
    }

    public static string CoinGeckoKey
    {
        get
        {
            if (configuration == null)
            {
                InitConfiguration();
            }

            string apiKey = configuration["CoinGecko:ApiKey"];
            return apiKey;
        }
    }

    public static string PricesToCheck
    {
        get
        {
            if (configuration == null)
            {
                InitConfiguration();
            }

            string PricesToCheck = configuration["Settings:PricesToCheck"];
            return PricesToCheck;
        }
    }

    public static List<XpubKeyPair> XPubKeys
    {
        get
        {
            if (configuration == null)
            {
                InitConfiguration();
            }

            IConfigurationSection xpubKeyPairsSection = configuration.GetSection("XpubKeyPairs");
            List<XpubKeyPair> xpubKeyPairs = xpubKeyPairsSection.GetChildren()
                                                  .Select(x => new XpubKeyPair
                                                  {
                                                      Xpub = x["Xpub"],
                                                      ScriptPubKeyType = ParseScriptPubKeyType(x["ScriptPubKeyType"])
                                                  })
                                                  .ToList();

            return xpubKeyPairs;

        }
    }

    private static ScriptPubKeyType ParseScriptPubKeyType(string scriptPubKeyType)
    {
        //ScriptPubKeyType.SegwitP2SH
        //    ScriptPubKeyType.Segwit
        //    ScriptPubKeyType.Legacy

        if (Enum.TryParse<ScriptPubKeyType>(scriptPubKeyType, out ScriptPubKeyType result))
        {
            return result;
        }

        // Handle the case where parsing fails, e.g., throw an exception or return a default value
        throw new ArgumentException($"Invalid ScriptPubKeyType: {scriptPubKeyType}");
    }



}