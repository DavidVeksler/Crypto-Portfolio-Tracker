# Crypto Portfolio Tracker

Crypto Portfolio Tracker is a free, open-source, cross-platform **cryptocurrency portfolio tracker** built as a .NET console application. It provides real-time tracking of Bitcoin and Ethereum **wallet balances** across multiple watch-only wallets: it fetches live price data from CoinGecko, uses BIP39 to derive your receiving addresses from a root public key (xpub/zpub), then queries balances from a randomly-picked Electrum server — without ever transmitting your extended public key to a third party.

Because it only needs an xpub/zpub (never a private key or seed phrase), it works as a **non-custodial, watch-only balance tracker** — it can see what your wallet holds but can never move funds. It runs anywhere the .NET runtime does: Windows, macOS, and Linux.

**TO GET STARTED**: rename `CryptoTracker.ConsoleApp/RENAMEME appsettings.json` to `appsettings.json` and put in your xpub/zpub and the tokens you want to track.

## Features

- Retrieves and displays current cryptocurrency prices and market data.
- *Privacy Focused:* Randomly picks an Electrum server and fetches balances from individual addresses.  Your xpubs are never shared.
- Tracks the value of Bitcoin and Ethereum wallets using extended public keys (xpubs & zpubs).
- Supports different wallet types (Segwit, Legacy, Bech32, etc.).
- Configurable to monitor a range of cryptocurrencies.
- Neatly formatted, auto-refreshing and easy-to-read output.

## Output Example

The application displays data in a well-organized table format, showing details like cryptocurrency prices, market cap, and wallet balances in both BTC and USD. 

![Wallet Balances](https://github.com/DavidVeksler/Crypto-Portfolio-Tracker/blob/main/Screenshots/WalletBalances2.png)

## Configuration

`appsettings.json` (in `CryptoTracker.ConsoleApp/`) allows you to set your CoinGecko API key and specify the cryptocurrencies and wallets you want to track.

### CoinGecko API Key

- **Optional**: [Get your API key here](https://support.coingecko.com/hc/en-us/articles/21880397454233).
- Add your API key to the `appsettings.json` file.

### Cryptocurrency Settings

- Add the symbols of the cryptocurrencies you want to track in the `PricesToCheck` setting.
- Full list of supported tokens: [CoinGecko API](https://api.coingecko.com/api/v3/coins/list)

### Wallet Settings

- Add your extended public keys (XPubKeys) to the `XpubKeyPairs` section.
- Specify the `ScriptPubKeyType` for each wallet (e.g., `SegwitP2SH`, `Legacy`, `SegwitBech32`).
- Track ERC-20 tokens (e.g. USDC, USDT) by adding Ethereum addresses under `Ethereum:AddressesToMonitor`.

## Installation and Usage

1. Clone the repository: `git clone https://github.com/DavidVeksler/Crypto-Portfolio-Tracker.git`
2. Rename `CryptoTracker.ConsoleApp/RENAMEME appsettings.json` to `appsettings.json` and fill in your xpub/zpub keys, Ethereum addresses, and API key.
3. Build and run with the [.NET SDK](https://dotnet.microsoft.com/download):
   ```sh
   dotnet build
   dotnet run --project CryptoTracker.ConsoleApp
   ```

## Tech Stack

- **Language/runtime:** C# on .NET 8, runs as a cross-platform console app
- **Bitcoin address derivation:** [NBitcoin](https://github.com/MetacoSA/NBitcoin) (BIP39/BIP32 xpub/zpub → addresses)
- **Ethereum:** [Nethereum](https://nethereum.com/)
- **Balance lookups:** [ElectrumXClient](https://github.com/xchwarze/ElectrumXClient) against public Electrum servers
- **Price data:** [CoinGecko API](https://www.coingecko.com/en/api)
- **Console UI:** Alba.CsConsoleFormat

## Dependencies

- .NET 8 SDK or runtime

## License

This project is open-source and available under [MIT License](LICENSE).
