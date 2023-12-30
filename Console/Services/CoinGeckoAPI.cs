﻿using System.Diagnostics;
using Newtonsoft.Json;
using System.Net.Http;

namespace Console.Services
{
    public class CoinGeckoService
    {
        private readonly HttpClient _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.coingecko.com/"),
            DefaultRequestHeaders = { { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36" } }
        };

        public async Task<string[]> GetSupportedVsCurrenciesAsync()
        {
            return await FetchAsync<string[]>("/api/v3/simple/supported_vs_currencies");
        }

        public async Task<CoinInfo[]> GetCurrencyInfoAsync(string vsCurrency, string ids)
        {
            string requestParams = $"/api/v3/coins/markets?vs_currency={vsCurrency}&ids={ids}";
            return await FetchAsync<CoinInfo[]>(requestParams);
        }

        private async Task<T> FetchAsync<T>(string requestUri)
        {
            var response = await _httpClient.GetAsync($"{requestUri}&x_cg_demo_api_key={Settings.CoinGeckoKey}");
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
