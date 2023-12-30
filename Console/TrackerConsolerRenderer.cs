using Alba.CsConsoleFormat;
using Console.Services;
using System;
using System.Linq;
using Alba.CsConsoleFormat;
using Console.Services;

namespace Console
{
    internal class TrackerConsolerRenderer
    {
        public static async void RenderCryptoTickerAsync()
        {
            var service = new CoinGeckoService();

            while (true)
            {
                var info = service.GetCurrencyInfoAsync("usd", Settings.PricesToCheck).Result;
                TrackerConsolerRenderer.RenderCryptoPrices(info);

                // Start a 30-second countdown
                for (int i = 30; i > 0; i--)
                {
                    System.Console.SetCursorPosition(0, System.Console.CursorTop);
                    System.Console.Write($"Refreshing in {i} seconds... ");
                    await Task.Delay(1000); // Wait for 1 second
                }

                System.Console.Clear(); // Clear the console for the next update
            }
        }



        public static void RenderCryptoPrices(CoinInfo[] coins)
        {
            var doc = new Document();

            var grid = new Grid
            {
                Columns =
            {
                GridLength.Auto,
                GridLength.Auto,
                GridLength.Auto,
                GridLength.Auto,
                GridLength.Auto,
                GridLength.Auto
            },
                Color = ConsoleColor.Gray
            };

            grid.Children.Add(new[]
            {
            new Cell("Name") { Color = ConsoleColor.Yellow },
            new Cell("Current Price") { Color = ConsoleColor.Yellow },
            new Cell("Market Cap") { Color = ConsoleColor.Yellow },
            new Cell("Rank") { Color = ConsoleColor.Yellow },
            new Cell("24h Change") { Color = ConsoleColor.Yellow },
            new Cell("24h Change %") { Color = ConsoleColor.Yellow }
        });

            foreach (var coin in coins)
            {
                var priceColor = coin.PriceChangePercentage24h >= 0 ? ConsoleColor.Green : ConsoleColor.Red;

                grid.Children.Add(new[]
                {
                new Cell(coin.Name),
                new Cell($"{coin.CurrentPrice:C}") { Color = priceColor },
                new Cell($"{coin.MarketCap:N0}"),
                new Cell(coin.MarketCapRank.ToString()),
                new Cell($"{coin.MarketCapChange24h:N2}%") { Color = coin.MarketCapChange24h >= 0 ? ConsoleColor.Green : ConsoleColor.Red },
                new Cell($"{coin.MarketCapChangePercentage24h/100:P2}") { Color = coin.MarketCapChange24h >= 0 ? ConsoleColor.Green : ConsoleColor.Red }
            });
            }

            doc.Children.Add(grid);
            ConsoleRenderer.RenderDocument(doc);
        }
    }




}

