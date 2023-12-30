using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;

namespace Console.Services
{
    public class CoinGeckoService
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.coingecko.com/api/v3"),
            DefaultRequestHeaders = { { "User-Agent", "Your User Agent" } }
        };

        public async Task<string[]> GetSupportedVsCurrenciesAsync()
        {
            return await FetchAsync<string[]>("/simple/supported_vs_currencies");
        }

        public async Task<CoinInfo[]> GetCurrencyInfoAsync(string vsCurrency, string ids)
        {
            string requestParams = $"/coins/markets?vs_currency={vsCurrency}&ids={ids}";
            return await FetchAsync<CoinInfo[]>(requestParams);
        }

        private async Task<T> FetchAsync<T>(string requestUri)
        {
            var response = await _httpClient.GetAsync(requestUri);
            string content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Error: {response.StatusCode}");
                Debug.WriteLine($"Response: {content}");
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode} and response: {content}");
            }
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
