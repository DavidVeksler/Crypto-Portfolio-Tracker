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
        string executablePath = AppDomain.CurrentDomain.BaseDirectory;

        // Navigate up from the executable path to the project root
        string projectRootPath = Directory.GetParent(executablePath).Parent?.Parent?.Parent?.FullName;
        string appSettingsPath = Path.Combine(projectRootPath, "appsettings.json");

        if (!File.Exists(appSettingsPath))
        {
            // If the appsettings.json file is not found in the project root, use the executable directory
            appSettingsPath = Path.Combine(executablePath, "appsettings.json");
            if (!File.Exists(appSettingsPath))
            {
                throw new FileNotFoundException("appsettings.json is required");
            }
        }

        IConfigurationBuilder builder = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(appSettingsPath))
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

    //Options for scriptPubKeyType: ScriptPubKeyType.SegwitP2SH
    //    ScriptPubKeyType.Segwit
    //    ScriptPubKeyType.Legacy
    private static ScriptPubKeyType ParseScriptPubKeyType(string scriptPubKeyType)
    {       

        if (Enum.TryParse<ScriptPubKeyType>(scriptPubKeyType, out ScriptPubKeyType result))
        {
            return result;
        }

        // Handle the case where parsing fails, e.g., throw an exception or return a default value
        throw new ArgumentException($"Invalid ScriptPubKeyType: {scriptPubKeyType}");
    }



}