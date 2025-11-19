using CryptoTracker.Core.Abstractions;
using CryptoTracker.Core.Configuration;
using CryptoTracker.Core.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace CryptoTracker.Core.Services.CryptoPriceServices;

/// <summary>
/// Implements cryptocurrency price lookup using the CoinGecko API.
/// </summary>
public class CoinGeckoService : ICryptoPriceService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CoinGeckoService> _logger;
    private readonly CoinGeckoOptions _options;

    public CoinGeckoService(HttpClient httpClient, IOptions<CryptoTrackerOptions> options, ILogger<CoinGeckoService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value.CoinGecko;

        _httpClient.BaseAddress = new Uri("https://api.coingecko.com/");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", AppConstants.Http.DefaultUserAgent);
    }

    public async Task<string[]> GetSupportedVsCurrenciesAsync()
    {
        return await FetchAsync<string[]>("/api/v3/simple/supported_vs_currencies");
    }

    public async Task<CoinInfo[]> GetCurrencyInfoAsync(string vsCurrency, string ids)
    {
        var requestParams = $"/api/v3/coins/markets?vs_currency={vsCurrency}&ids={ids}";
        return await FetchAsync<CoinInfo[]>(requestParams);
    }

    private async Task<T> FetchAsync<T>(string requestUri)
    {
        var uri = _options.ApiKey != null
            ? $"{requestUri}&x_cg_demo_api_key={_options.ApiKey}"
            : requestUri;

        _logger.LogDebug("Fetching data from CoinGecko: {Uri}", uri);

        var response = await _httpClient.GetAsync(uri);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("CoinGecko API request failed. Status: {StatusCode}, Response: {Response}",
                response.StatusCode, content);
            throw new HttpRequestException(
                $"Request failed with status code: {response.StatusCode} and response: {content}");
        }

        _logger.LogDebug("Successfully fetched data from CoinGecko");
        return JsonConvert.DeserializeObject<T>(content)
            ?? throw new InvalidOperationException("Failed to deserialize response");
    }
}