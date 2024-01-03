# Crypto Portfolio Tracker

Crypto Portfolio Tracker provides real-time tracking of cryptocurrency balances across different wallets.  Fetches price data from CoinGecko then uses BIP39 to generate your addresses from your root public key, then fetches the balances from a randomly-picked Electrum server.

It is designed to run on Windows, Mac, and Linux platforms.

**TO GET STARTED**: RENAME "RENAMEME appsettings.json" to "appsettings.json" and put in your xpub/zpub and the tokens you want to track. 

## Features

- Retrieves and displays current cryptocurrency prices and market data.
- *Privacy Focused:* Randomly picks an Electrum server and fetches balances from individual addresses.  Your xpubs are never shared.
- Tracks the value of Bitcoin wallets using extended public keys (xpubs & zpubs).
- Supports different wallet types (Segwit, Legacy, Bech32, etc.).
- Configurable to monitor a range of cryptocurrencies.
- Neatly formatted, auto-refreshing and easy-to-read output.

## Output Example

The application displays data in a well-organized table format, showing details like cryptocurrency prices, market cap, and wallet balances in both BTC and USD. 

![Wallet Balances](https://github.com/DavidVeksler/Crypto-Portfolio-Tracker/blob/main/Screenshots/WalletBalances1.png)

## Configuration

`appsettings.config` allows you to set your CoinGecko API key and specify the cryptocurrencies and wallets you want to track.

### CoinGecko API Key

- **Optional**: [Get your API key here](https://support.coingecko.com/hc/en-us/articles/21880397454233).
- Add your API key to the `appsettings.config` file.

### Cryptocurrency Settings

- Add the symbols of the cryptocurrencies you want to track in the `PricesToCheck` setting.
- Full list of supported tokens: [CoinGecko API](https://api.coingecko.com/api/v3/coins/list)

### Wallet Settings

- Add your extended public keys (XPubKeys) to the `XpubKeyPairs` section.
- Specify the `ScriptPubKeyType` for each wallet (e.g., `SegwitP2SH`, `Legacy`, `SegwitBech32`).

## Installation and Usage

1. Clone or download the repository.
2. Configure `appsettings.config` with your API key and wallet information.
3. Run the application from your preferred terminal or command line interface.

## Dependencies

- .NET Core runtime

## License

This project is open-source and available under [MIT License](LICENSE).
