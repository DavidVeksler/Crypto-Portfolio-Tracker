namespace CryptoTracker.Core.Constants;

/// <summary>
/// Application-wide constants for configuration and defaults.
/// </summary>
public static class AppConstants
{
    /// <summary>
    /// Blockchain and wallet configuration constants.
    /// </summary>
    public static class Blockchain
    {
        /// <summary>
        /// Number of consecutive unused addresses before stopping the search.
        /// </summary>
        public const int AddressGapLimit = 10;

        /// <summary>
        /// Cache timeout duration for address indices in minutes.
        /// </summary>
        public const int AddressIndexCacheTimeoutMinutes = 30;
    }

    /// <summary>
    /// User interface configuration constants.
    /// </summary>
    public static class UI
    {
        /// <summary>
        /// Default refresh interval in seconds for the crypto tracker display.
        /// </summary>
        public const int RefreshIntervalSeconds = 30;
    }

    /// <summary>
    /// HTTP client configuration constants.
    /// </summary>
    public static class Http
    {
        /// <summary>
        /// Default user agent for HTTP requests.
        /// </summary>
        public const string DefaultUserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
    }
}
