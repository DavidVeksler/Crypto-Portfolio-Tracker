using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using NBitcoin;


public class XpubKeyPair
{
    public string Xpub { get; set; }
    public ScriptPubKeyType ScriptPubKeyType { get; set; }
}

public class Settings
{
    internal static IConfigurationRoot configuration;

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


        var builder = new ConfigurationBuilder()
        .SetBasePath(path)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        configuration = builder.Build();

    }

    public static string OpenAIKey
    {
        get
        {
            if (configuration == null) InitConfiguration();
            string apiKey = configuration["OpenAI:ApiKey"];
            return apiKey;
        }
    }

    public static string CoinGeckoKey
    {
        get
        {
            if (configuration == null) InitConfiguration();
            string apiKey = configuration["CoinGecko:ApiKey"];
            return apiKey;
        }
    }

    public static string PricesToCheck
    {
        get
        {
            if (configuration == null) InitConfiguration();
            string PricesToCheck = configuration["Settings:PricesToCheck"];
            return PricesToCheck;
        }
    }

    public static List<XpubKeyPair> XPubKeys
    {
        get
        {
            if (configuration == null) InitConfiguration();            
            var xpubKeyPairsSection = configuration.GetSection("XpubKeyPairs");
            var xpubKeyPairs = xpubKeyPairsSection.GetChildren()
                                                  .Select(x => new XpubKeyPair
                                                  {
                                                      Xpub = x["Xpub"],
                                                      ScriptPubKeyType = Enum.TryParse<ScriptPubKeyType>(x["ScriptPubKeyType"], out var result)
                                                     ? result
                                                     : throw new ArgumentException($"Invalid ScriptPubKeyType: {x["ScriptPubKeyType"]}")
                                                  })
                                                  .ToList();

            return xpubKeyPairs;

        }
    }

    

}