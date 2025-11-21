using Alba.CsConsoleFormat;
using CryptoTracker.ConsoleApp.Abstractions;
using CryptoTracker.Core.Services.CryptoPriceServices;
using Document = Alba.CsConsoleFormat.Document;

namespace CryptoTracker.ConsoleApp.Rendering;

/// <summary>
/// Console renderer for cryptocurrency price information.
/// </summary>
public class TrackerConsoleRenderer : IConsoleRenderer
{
    public void RenderCryptoPrices(CoinGeckoMarketData[] coins)
    {
        Document doc = new();

        Grid grid = new()
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

        grid.Children.Add(new Cell("Name") { Color = ConsoleColor.Yellow },
            new Cell("Current Price") { Color = ConsoleColor.Yellow },
            new Cell("Market Cap") { Color = ConsoleColor.Yellow }, new Cell("Rank") { Color = ConsoleColor.Yellow },
            new Cell("24h Change") { Color = ConsoleColor.Yellow },
            new Cell("24h Change %") { Color = ConsoleColor.Yellow });

        foreach (var coin in coins)
        {
            var priceColor = coin.PriceChangePercentage24h >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
            grid.Children.Add(new Cell(coin.Name), new Cell($"{coin.CurrentPrice:C}") { Color = priceColor },
                new Cell($"{coin.MarketCap:N0}"), new Cell(coin.MarketCapRank.ToString()),
                new Cell($"{coin.MarketCapChange24h:N2}%")
                    { Color = coin.MarketCapChange24h >= 0 ? ConsoleColor.Green : ConsoleColor.Red },
                new Cell($"{coin.MarketCapChangePercentage24h / 100:P2}")
                    { Color = coin.MarketCapChange24h >= 0 ? ConsoleColor.Green : ConsoleColor.Red });
        }

        doc.Children.Add(grid);
        ConsoleRenderer.RenderDocument(doc);
    }
}