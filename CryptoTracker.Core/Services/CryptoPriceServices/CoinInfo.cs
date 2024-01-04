using Newtonsoft.Json;

namespace Console.Services
{
    public class CoinInfo
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("symbol")]
        public required string Symbol { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("image")]
        public required string Image { get; set; }

        [JsonProperty("current_price")]
        public decimal CurrentPrice { get; set; }

        [JsonProperty("market_cap")]
        public long MarketCap { get; set; }

        [JsonProperty("market_cap_rank")]
        public int MarketCapRank { get; set; }

        [JsonProperty("fully_diluted_valuation")]
        public long FullyDilutedValuation { get; set; }

        [JsonProperty("total_volume")]
        public long TotalVolume { get; set; }

        [JsonProperty("high_24h")]
        public decimal High24h { get; set; }

        [JsonProperty("low_24h")]
        public decimal Low24h { get; set; }

        [JsonProperty("price_change_24h")]
        public decimal PriceChange24h { get; set; }

        [JsonProperty("price_change_percentage_24h")]
        public decimal PriceChangePercentage24h { get; set; }

        [JsonProperty("market_cap_change_24h")]
        public long MarketCapChange24h { get; set; }

        [JsonProperty("market_cap_change_percentage_24h")]
        public decimal MarketCapChangePercentage24h { get; set; }

        [JsonProperty("circulating_supply")]
        public double CirculatingSupply { get; set; }

        [JsonProperty("total_supply")]
        public double? TotalSupply { get; set; }

        [JsonProperty("max_supply")]
        public double? MaxSupply { get; set; }

        [JsonProperty("ath")]
        public decimal Ath { get; set; }

        [JsonProperty("ath_change_percentage")]
        public decimal AthChangePercentage { get; set; }

        [JsonProperty("ath_date")]
        public DateTime AthDate { get; set; }

        [JsonProperty("atl")]
        public decimal Atl { get; set; }

        [JsonProperty("atl_change_percentage")]
        public decimal AtlChangePercentage { get; set; }

        [JsonProperty("atl_date")]
        public DateTime AtlDate { get; set; }

        [JsonProperty("roi")]
        public required object Roi { get; set; } // Adjust as needed.

        [JsonProperty("last_updated")]
        public DateTime LastUpdated { get; set; }
    }
}