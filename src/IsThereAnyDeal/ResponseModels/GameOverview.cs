using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace IsThereAnyDeal.ResponseModels
{
    internal class GameOverview
    {
        [JsonProperty(".meta")]
        public GameOverviewMeta Meta { get; set; } = null!;
        public Dictionary<string, GameOverviewData> Data { get; set; } = null!;

        public Models.GameOverview? Extract(string plain)
        {
            if (!Data.TryGetValue(plain, out GameOverviewData? gameOverviewData) || gameOverviewData.Price is null)
            {
                return null;
            }

            return new(
                new Models.GameBestCurrentPrice(
                    gameOverviewData.Price.Store,
                    gameOverviewData.Price.Price,
                    gameOverviewData.Price.Cut,
                    gameOverviewData.Price.Url
                    ),
                gameOverviewData.Lowest is null ? null : new Models.GameLowestHistoricalPrice(
                    gameOverviewData.Lowest.Store,
                    gameOverviewData.Lowest.Price,
                    gameOverviewData.Lowest.Cut,
                    gameOverviewData.Lowest.Recorded,
                    gameOverviewData.Lowest.FormattedRecorded
                    )
                );
        }
    }

    internal class GameOverviewMeta
    {
        public string Region { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string Currency { get; set; } = null!;
    }

    internal class GameOverviewData
    {
        public GameOverviewPrice? Price { get; set; } = null!;
        public GameOverviewLowestPrice? Lowest { get; set; } = null!;
        public GameOverviewUrls Urls { get; set; } = null!;
    }

    internal class GameOverviewPrice
    {
        public string Store { get; set; } = null!;
        public int Cut { get; set; }
        public decimal Price { get; set; }
        [JsonProperty("price_formatted")]
        public string FormattedPrice { get; set; } = null!;
        public string Url { get; set; } = null!;
        public List<string> Drm { get; set; } = null!;
    }

    internal class GameOverviewLowestPrice
    {
        public string Store { get; set; } = null!;
        public int Cut { get; set; }
        public decimal Price { get; set; }
        [JsonProperty("price_formatted")]
        public string FormattedPrice { get; set; } = null!;
        public string Url { get; set; } = null!;
        [JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Recorded { get; set; }
        [JsonProperty("recorded_formatted")]
        public string FormattedRecorded { get; set; } = null!;
    }

    internal class GameOverviewUrls
    {
        public string Info { get; set; } = null!;
        public string History { get; set; } = null!;
        public string Bundles { get; set; } = null!;
    }
}
