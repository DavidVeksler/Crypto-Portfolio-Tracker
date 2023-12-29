using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console.Services
{
    //[{"id":"bitcoin","symbol":"btc","name":"Bitcoin","image":"https://assets.coingecko.com/coins/images/1/large/bitcoin.png?1696501400","current_price":41929,"market_cap":819830485967,"market_cap_rank":1,"fully_diluted_valuation":879106053565,"total_volume":23079326690,"high_24h":43097,"low_24h":41861,"price_change_24h":-678.7354517800632,"price_change_percentage_24h":-1.593,"market_cap_change_24h":-12505895200.949707,"market_cap_change_percentage_24h":-1.5025,"circulating_supply":19584031.0,"total_supply":21000000.0,"max_supply":21000000.0,"ath":69045,"ath_change_percentage":-39.09085,"ath_date":"2021-11-10T14:24:11.849Z","atl":67.81,"atl_change_percentage":61919.17889,"atl_date":"2013-07-06T00:00:00.000Z","roi":null,"last_updated":"2023-12-29T17:18:40.888Z"}]



    using System;
    using Newtonsoft.Json;

    public class CoinInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

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
        public object Roi { get; set; } // Adjust as needed.

        [JsonProperty("last_updated")]
        public DateTime LastUpdated { get; set; }
    }



}
