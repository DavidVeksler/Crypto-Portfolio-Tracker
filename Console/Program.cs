


// See https://aka.ms/new-console-template for more information
using Console.Services;

System.Console.WriteLine("Hello, World!");

var service = new CoinGeckoService();

var info = service.GetCurrencyInfoAsync("usd", "bitcoin").Result;

System.Console.WriteLine(info);
