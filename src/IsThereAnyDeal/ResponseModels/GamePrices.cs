using Newtonsoft.Json;

namespace IsThereAnyDeal.ResponseModels
{
    internal class GamePrices
    {
        [JsonProperty(".meta")]
        public GamePricesMeta Meta { get; set; } = null!;
        public Dictionary<string, GamePricesData> Data { get; set; } = null!;

        public List<Models.GamePrice>? Extract(string plain)
        {
            if (!Data.TryGetValue(plain, out GamePricesData? gamePricesData))
            {
                return null;
            }

            return gamePricesData.PriceList.ConvertAll(
                price => new Models.GamePrice(price.NewPrice, price.OldPrice, price.PriceCut, price.Shop.Name, price.Url)
                );
        }
    }

    internal class GamePricesMeta
    {
        public string Currency { get; set; } = null!;
    }

    internal class GamePricesData
    {
        [JsonProperty("list")]
        public List<GamePricesPrice> PriceList { get; set; } = null!;
        public GamePricesUrls Urls { get; set; } = null!;
    }

    internal class GamePricesPrice
    {
        [JsonProperty("price_new")]
        public decimal NewPrice { get; set; }
        [JsonProperty("price_old")]
        public decimal OldPrice { get; set; }
        [JsonProperty("price_cut")]

        public int PriceCut { get; set; }
        public string Url { get; set; } = null!;
        public GamePricesShop Shop { get; set; } = null!;
        public List<string> Drm { get; set; } = null!;
    }

    internal class GamePricesShop
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    internal class GamePricesUrls
    {
        public string Game { get; set; } = null!;
    }
}
