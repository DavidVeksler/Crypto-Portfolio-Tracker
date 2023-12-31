using Microsoft.Extensions.Configuration;
using NBitcoin;

public class XpubKeyPair
{
    public string Xpub { get; set; }
    public ScriptPubKeyType ScriptPubKeyType { get; set; }
}

public static class ConfigSettings
{
    private static IConfigurationRoot? _configuration;

    static ConfigSettings()
    {
        InitializeConfiguration();
    }

    private static void InitializeConfiguration()
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string appSettingsPath = FindAppSettingsPath(basePath);

        _configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(appSettingsPath))
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();
    }

    private static string FindAppSettingsPath(string basePath)
    {
        string[] potentialPaths = { basePath, Directory.GetParent(basePath)?.Parent?.Parent?.Parent?.FullName };

        foreach (var path in potentialPaths)
        {
            var appSettingsFilePath = Path.Combine(path, "appsettings.json");
            if (File.Exists(appSettingsFilePath))
            {
                return appSettingsFilePath;
            }
        }

        throw new FileNotFoundException("appsettings.json is required");
    }

    private static string GetConfigValue(string key) => _configuration[key];

    public static string OpenAIKey => GetConfigValue("OpenAI:ApiKey");
    public static string CoinGeckoKey => GetConfigValue("CoinGecko:ApiKey");
    public static string PricesToCheck => GetConfigValue("Settings:PricesToCheck");

    public static List<XpubKeyPair> XPubKeys =>
        _configuration.GetSection("XpubKeyPairs").GetChildren()
                      .Select(c => new XpubKeyPair
                      {
                          Xpub = c["Xpub"],
                          ScriptPubKeyType = Enum.Parse<ScriptPubKeyType>(c["ScriptPubKeyType"])
                      })
                      .ToList();
}
