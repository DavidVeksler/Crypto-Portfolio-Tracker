using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Console.Services
{
    public class CoinGeckoService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.coingecko.com/api/v3";

        public CoinGeckoService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
        }

        public async Task<string[]> GetSupportedVsCurrenciesAsync()
        {
            string url = $"{BaseUrl}/simple/supported_vs_currencies";
            string response = await _httpClient.GetStringAsync(url);
            return JsonConvert.DeserializeObject<string[]>(response);
        }

        public async Task<CoinInfo[]> GetCurrencyInfoAsync(string vsCurrency, string ids)
        {
            string requestParams = $"/coins/markets?vs_currency={vsCurrency}&ids={ids}&x_cg_demo_api_key={Settings.CoinGeckoKey}";
            string url = $"{BaseUrl}{requestParams}";


            // Send the request and get the response
            HttpResponseMessage response = await _httpClient.GetAsync(url);

            // Read the content as a string regardless of the response status
            string content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Log the status code and the content for debugging
                Debug.WriteLine($"Error: {response.StatusCode}");
                Debug.WriteLine($"Response: {content}");

                // You can throw an exception or handle the error based on your application's needs
                throw new HttpRequestException($"Request failed with status code: {response.StatusCode} and response: {content}");
            }

            var info = JsonConvert.DeserializeObject<CoinInfo[]>(content);

            return info;
        }
    }
}