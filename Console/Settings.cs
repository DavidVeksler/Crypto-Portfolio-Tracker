using Microsoft.Extensions.Configuration;



public class Settings
{
    internal static IConfigurationRoot configuration;

    private Settings()
    {

    }

    private static void InitConfiguration()
    {
        var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
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

}