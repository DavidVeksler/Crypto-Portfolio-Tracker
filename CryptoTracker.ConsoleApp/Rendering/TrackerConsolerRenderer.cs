using Alba.CsConsoleFormat;
using CryptoTracker.ConsoleApp.Abstractions;
using CryptoTracker.Core.Services.CryptoPriceServices;
using Document = Alba.CsConsoleFormat.Document;

namespace CryptoTracker.ConsoleApp.Rendering;

/// <summary>
/// Functional console renderer for cryptocurrency price information.
/// Separates pure data transformation from I/O side effects.
/// </summary>
public class TrackerConsoleRenderer : IConsoleRenderer
{
    /// <summary>
    /// Public interface: Composes pure document building with isolated I/O rendering.
    /// </summary>
    public void RenderCryptoPrices(CoinGeckoMarketData[] coins)
    {
        var document = BuildCryptoPricesDocument(coins);
        RenderToConsole(document); // I/O side effect isolated to single point
    }

    /// <summary>
    /// Pure function: Builds document structure from coin data with no side effects.
    /// All mutations happen within this function's scope - no external state modified.
    /// </summary>
    private static Document BuildCryptoPricesDocument(CoinGeckoMarketData[] coins)
    {
        var grid = CreateGrid();
        var headerCells = CreateHeaderCells();
        var dataCells = CreateDataCells(coins);

        // Functional composition: Combine header and data cells
        var allCells = headerCells.Concat(dataCells);

        // Add all cells to grid (contained mutation within pure function)
        foreach (var cell in allCells)
            grid.Children.Add(cell);

        var doc = new Document();
        doc.Children.Add(grid);
        return doc;
    }

    /// <summary>
    /// Pure function: Creates grid structure with immutable configuration.
    /// </summary>
    private static Grid CreateGrid() => new()
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

    /// <summary>
    /// Pure function: Creates header cells as immutable collection.
    /// </summary>
    private static IEnumerable<Cell> CreateHeaderCells() => new[]
    {
        new Cell("Name") { Color = ConsoleColor.Yellow },
        new Cell("Current Price") { Color = ConsoleColor.Yellow },
        new Cell("Market Cap") { Color = ConsoleColor.Yellow },
        new Cell("Rank") { Color = ConsoleColor.Yellow },
        new Cell("24h Change") { Color = ConsoleColor.Yellow },
        new Cell("24h Change %") { Color = ConsoleColor.Yellow }
    };

    /// <summary>
    /// Pure function: Transforms coin data to cells using LINQ (no mutable collections).
    /// </summary>
    private static IEnumerable<Cell> CreateDataCells(CoinGeckoMarketData[] coins) =>
        coins.SelectMany(CreateCellsForCoin);

    /// <summary>
    /// Pure function: Maps a single coin to its row cells using functional transformations.
    /// </summary>
    private static IEnumerable<Cell> CreateCellsForCoin(CoinGeckoMarketData coin)
    {
        var priceColor = DeterminePriceColor(coin.PriceChangePercentage24h);
        var changeColor = DeterminePriceColor(coin.MarketCapChange24h);

        return new[]
        {
            new Cell(coin.Name),
            new Cell($"{coin.CurrentPrice:C}") { Color = priceColor },
            new Cell($"{coin.MarketCap:N0}"),
            new Cell(coin.MarketCapRank.ToString()),
            new Cell($"{coin.MarketCapChange24h:N2}%") { Color = changeColor },
            new Cell($"{coin.MarketCapChangePercentage24h / 100:P2}") { Color = changeColor }
        };
    }

    /// <summary>
    /// Pure function: Determines color based on value (no side effects).
    /// </summary>
    private static ConsoleColor DeterminePriceColor(decimal value) =>
        value >= 0 ? ConsoleColor.Green : ConsoleColor.Red;

    /// <summary>
    /// I/O boundary: Isolated side effect for rendering to console.
    /// All impure operations (console output) contained here.
    /// </summary>
    private static void RenderToConsole(Document document) =>
        ConsoleRenderer.RenderDocument(document);
}