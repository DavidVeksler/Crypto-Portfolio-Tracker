using Microsoft.Extensions.Configuration;
using NBitcoin;

namespace CryptoTracker.Core.Infrastructure.Configuration
{

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

            foreach (string path in potentialPaths)
            {
                string appSettingsFilePath = Path.Combine(path, "appsettings.json");
                if (File.Exists(appSettingsFilePath))
                {
                    return appSettingsFilePath;
                }
            }

            throw new FileNotFoundException("appsettings.json is required");
        }

        private static string GetConfigValue(string key)
        {
            return _configuration[key];
        }

        public static string OpenAIKey => GetConfigValue("OpenAI:ApiKey");
        public static string CoinGeckoKey => GetConfigValue("CoinGecko:ApiKey");
        public static string PricesToCheck => GetConfigValue("PricesToCheck");
        public static string InfuraKey => GetConfigValue("Infura:ApiKey");
        public static string EtherscanKey => GetConfigValue("Etherscan:ApiKey");



        public static List<string> EthereumAddressesToMonitor
        {
            get
            {
                var addressSections = _configuration.GetSection("Ethereum:AddressesToMonitor").GetChildren();
                var addresses = addressSections.Select(section => section.Value).ToList();
                return addresses;
            }
        }

        public static IEnumerable<string> EthereumTokensToTrack
        {
            get
            {
                var tokens = GetConfigValue("Ethereum:TokensToTrack");
                return tokens?.Split(',') ?? Enumerable.Empty<string>();
            }
        }


        public static List<XpubKeyPair> XPubKeys =>
            _configuration.GetSection("XpubKeyPairs").GetChildren()
                          .Select(c => new XpubKeyPair
                          {
                              Xpub = c["Xpub"],
                              ScriptPubKeyType = Enum.Parse<ScriptPubKeyType>(c["ScriptPubKeyType"])
                          })
                          .ToList();


    }
}