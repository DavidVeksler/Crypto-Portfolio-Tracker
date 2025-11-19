using CryptoTracker.ConsoleApp.CoinStatusRenderingService;
using CryptoTracker.ConsoleApp.Services;
using CryptoTracker.Core.Abstractions;
using CryptoTracker.Core.Configuration;
using CryptoTracker.Core.Services.Bitcoin;
using CryptoTracker.Core.Services.CryptoPriceServices;
using CryptoTracker.Core.Services.Electrum;
using CryptoTracker.Core.Services.EthereumBalanceServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Configure configuration
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Configure options
builder.Services.Configure<CryptoTrackerOptions>(builder.Configuration);

// Configure logging
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

// Register HTTP clients
builder.Services.AddHttpClient<ICryptoPriceService, CoinGeckoService>();
builder.Services.AddHttpClient<IBatchEthereumBalanceService, EtherscanBalanceService>();

// Register services
builder.Services.AddSingleton<IElectrumClientProvider, ElectrumServerProvider>();
builder.Services.AddSingleton<IWalletTracker, ElectrumCryptoWalletTracker>();
builder.Services.AddSingleton<IEthereumBalanceService, InfuraBalanceLookupService>();
builder.Services.AddSingleton<IConsoleRenderer, TrackerConsoleRenderer>();

// Register application
builder.Services.AddSingleton<CryptoTrackerApplication>();

var host = builder.Build();

// Run the application
var app = host.Services.GetRequiredService<CryptoTrackerApplication>();
await app.RunAsync();